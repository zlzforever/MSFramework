using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MSFramework.AspNetCore.Test;

/// <summary>
///     上传存储扩展测试：验证新存储结构「真实文件存于 upload/{date}/{l1}/{l2}，oss/{l1}/{l2} 仅作判重链接」，
///     以及跨日期重复上传零数据写、判重命中、并发链接复用、符号链接失败降级复制等关键行为
/// </summary>
public class FormFileTests : BaseTest, IDisposable
{
    /// <summary>
    ///     计算内容字节的 MD5 大写十六进制值（与框架存储命名规则一致）
    /// </summary>
    /// <param name="content">内容字节</param>
    /// <returns>大写十六进制 MD5 字符串</returns>
    private static string ComputeMd5(byte[] content) => Convert.ToHexString(MD5.HashData(content));

    /// <summary>
    ///     构造指定内容的 FormFile 测试对象
    /// </summary>
    /// <param name="content">文件内容字节</param>
    /// <param name="name">文件名（含扩展名）</param>
    /// <returns>IFormFile 实例</returns>
    private static IFormFile CreateFormFile(byte[] content, string name = "1.csv")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "1", name);
    }

    /// <summary>
    ///     计算新方案下真实文件应落盘的完整路径：wwwroot/upload/{date}/{l1}/{l2}/{md5}{ext}
    /// </summary>
    /// <param name="content">文件内容字节</param>
    /// <param name="name">文件名（含扩展名）</param>
    /// <returns>真实文件完整路径</returns>
    private static string RealFilePath(byte[] content, string name = "1.csv")
    {
        var md5 = ComputeMd5(content);
        var fileName = $"{md5}{Path.GetExtension(name)}";
        return Path.Combine(AppContext.BaseDirectory, "wwwroot", "upload",
            DateTimeOffset.UtcNow.ToString("yyyyMMdd"), md5.Substring(0, 2), md5.Substring(2, 2), fileName);
    }

    /// <summary>
    ///     计算新方案下 oss 判重链接的完整路径：wwwroot/oss/{l1}/{l2}/{md5}{ext}
    /// </summary>
    /// <param name="content">文件内容字节</param>
    /// <param name="name">文件名（含扩展名）</param>
    /// <returns>oss 链接完整路径</returns>
    private static string OssLinkPath(byte[] content, string name = "1.csv")
    {
        var md5 = ComputeMd5(content);
        var fileName = $"{md5}{Path.GetExtension(name)}";
        return Path.Combine(Defaults.LocalOSSDirectory, md5.Substring(0, 2), md5.Substring(2, 2), fileName);
    }

    [Fact]
    public async Task SaveFile()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        var formFile = new FormFile(stream, 0, stream.Length, "1", "1.csv");
        await formFile.SaveAsync();
    }

    /// <summary>
    ///     首次上传：真实文件写入 upload/{date}/{l1}/{l2} 二级分层目录，oss 路径为指向真实文件的符号链接
    /// </summary>
    [Fact]
    public async Task FirstUploadWritesRealFileUnderDateLevelDirsAndCreatesOssLink()
    {
        var content = Encoding.UTF8.GetBytes($"first-upload-{Guid.NewGuid()}");

        var result = await CreateFormFile(content).SaveAsync();

        var realPath = RealFilePath(content);
        var ossPath = OssLinkPath(content);
        var md5 = ComputeMd5(content);
        var today = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        // 真实文件：upload/{date}/{l1}/{l2}/{md5}{ext}，为普通文件且内容正确
        Assert.True(File.Exists(realPath));
        Assert.False(File.GetAttributes(realPath).HasFlag(FileAttributes.ReparsePoint));
        Assert.Equal(content, await File.ReadAllBytesAsync(realPath));
        // oss 路径：符号链接，指向真实文件
        Assert.True(File.Exists(ossPath));
        Assert.True(File.GetAttributes(ossPath).HasFlag(FileAttributes.ReparsePoint));
        Assert.Equal(realPath, File.ResolveLinkTarget(ossPath, returnFinalTarget: true)?.FullName);
        // 返回语义：Path 为 upload 相对真实路径，PhysicalPath 为 oss 链接路径
        Assert.Equal(Path.Combine("upload", today, md5.Substring(0, 2), md5.Substring(2, 2), $"{md5}.csv"),
            result.Path);
        Assert.Equal(ossPath, result.PhysicalPath);
    }

    /// <summary>
    ///     同日期判重命中：相同内容重复上传不改写真实文件（零数据写），oss 链接已存在则直接复用
    /// </summary>
    [Fact]
    public async Task DuplicateUploadSameContentDoesNotRewriteDataAndReusesLink()
    {
        var content = Encoding.UTF8.GetBytes($"duplicate-{Guid.NewGuid()}");
        var first = await CreateFormFile(content).SaveAsync();
        var ossPath = first.PhysicalPath;
        var realPath = File.ResolveLinkTarget(ossPath, returnFinalTarget: true)?.FullName;
        Assert.NotNull(realPath);
        var beforeBytes = await File.ReadAllBytesAsync(realPath);
        var beforeWriteTime = File.GetLastWriteTimeUtc(realPath);

        var second = await CreateFormFile(content).SaveAsync();

        // 真实文件未被改写（零数据写）
        Assert.Equal(beforeBytes, await File.ReadAllBytesAsync(realPath));
        Assert.Equal(beforeWriteTime, File.GetLastWriteTimeUtc(realPath));
        // 真实文件仍为普通文件，oss 链接仍为符号链接（已存在则复用，不重复创建）
        Assert.False(File.GetAttributes(realPath).HasFlag(FileAttributes.ReparsePoint));
        Assert.True(File.GetAttributes(ossPath).HasFlag(FileAttributes.ReparsePoint));
        // 判重命中后返回路径与首次一致（同一内容同一日期）
        Assert.Equal(first.Path, second.Path);
        Assert.Equal(first.PhysicalPath, second.PhysicalPath);
    }

    /// <summary>
    ///     跨日期重复上传：判重键为 oss 链接（内容指纹恒定），仅创建新 upload 符号链接指向既有真实文件，零数据写
    /// </summary>
    [Fact]
    public async Task CrossDateDuplicateUploadCreatesOnlyNewLinkWithoutDataWrite()
    {
        var content = Encoding.UTF8.GetBytes($"cross-date-{Guid.NewGuid()}");
        var md5 = ComputeMd5(content);
        var fileName = $"{md5}.csv";
        var level1 = md5.Substring(0, 2);
        var level2 = md5.Substring(2, 2);
        // 预置更早日期的真实文件与 oss 判重链接（模拟历史上传，今日路径尚不存在）
        const string oldDate = "20200101";
        var oldRealDir = Path.Combine(AppContext.BaseDirectory, "wwwroot", "upload", oldDate, level1, level2);
        var oldRealPath = Path.Combine(oldRealDir, fileName);
        Directory.CreateDirectory(oldRealDir);
        await File.WriteAllBytesAsync(oldRealPath, content);
        var ossDir = Path.Combine(Defaults.LocalOSSDirectory, level1, level2);
        Directory.CreateDirectory(ossDir);
        var ossPath = Path.Combine(ossDir, fileName);
        File.CreateSymbolicLink(ossPath, oldRealPath);

        var result = await CreateFormFile(content).SaveAsync();

        // 今日 upload 路径仅创建符号链接指向旧真实文件，不写入任何数据
        var today = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var newLinkPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "upload", today, level1, level2, fileName);
        Assert.True(File.Exists(newLinkPath));
        Assert.True(File.GetAttributes(newLinkPath).HasFlag(FileAttributes.ReparsePoint));
        Assert.Equal(oldRealPath, File.ResolveLinkTarget(newLinkPath, returnFinalTarget: true)?.FullName);
        // 旧真实文件零数据写
        Assert.Equal(content, await File.ReadAllBytesAsync(oldRealPath));
        // 返回路径为今日真实路径（保持真实文件语义）
        Assert.Equal(Path.Combine("upload", today, level1, level2, fileName), result.Path);
        Assert.Equal(ossPath, result.PhysicalPath);
    }

    /// <summary>
    ///     并发上传相同内容：全部成功且无异常，真实文件内容正确，oss 链接唯一且有效
    ///     （数据写由 FileMode.Create 幂等保证，链接创建竞态由 catch 复用既有链接保证）
    /// </summary>
    [Fact]
    public async Task ConcurrentSameContentUploadsAllSucceedAndProduceSingleValidLink()
    {
        var content = Encoding.UTF8.GetBytes($"concurrent-{Guid.NewGuid()}");
        const int count = 8;
        var tasks = Enumerable.Range(0, count)
            .Select(i => CreateFormFile(content, $"f{i}.csv").SaveAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(count, results.Length);
        Assert.All(results, r => Assert.Equal(results[0].PhysicalPath, r.PhysicalPath));
        var ossPath = results[0].PhysicalPath;
        var realPath = File.ResolveLinkTarget(ossPath, returnFinalTarget: true)?.FullName;
        Assert.NotNull(realPath);
        Assert.Equal(content, await File.ReadAllBytesAsync(realPath));
    }

    /// <summary>
    ///     符号链接创建失败（文件系统不支持等场景）：降级为文件复制，oss 路径为内容一致的普通文件副本
    /// </summary>
    [Fact]
    public async Task SymbolicLinkFailureFallsBackToFileCopy()
    {
        var content = Encoding.UTF8.GetBytes($"fallback-{Guid.NewGuid()}");
        var original = FormFileExtensions.LinkCreator;
        FormFileExtensions.LinkCreator = (_, _) => throw new IOException("simulated: symlink unsupported");
        try
        {
            var result = await CreateFormFile(content).SaveAsync();

            // 降级为文件复制：oss 路径为普通文件（非符号链接），内容与真实文件一致
            var ossPath = result.PhysicalPath;
            Assert.True(File.Exists(ossPath));
            Assert.False(File.GetAttributes(ossPath).HasFlag(FileAttributes.ReparsePoint));
            Assert.Equal(content, await File.ReadAllBytesAsync(ossPath));
            Assert.Equal(content, await File.ReadAllBytesAsync(RealFilePath(content)));
        }
        finally
        {
            FormFileExtensions.LinkCreator = original;
        }
    }

    /// <summary>
    ///     生成唯一临时目录路径（并登记回收），避免测试间缓存与文件系统状态互相干扰
    /// </summary>
    /// <returns>唯一临时目录完整路径</returns>
    private static string NewTempDirPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "msf-ensure-dir", Guid.NewGuid().ToString("N"));
        _tempDirPaths.Add(path);
        return path;
    }

    /// <summary>
    ///     本测试类产生的临时目录集合（测试结束后统一清理）
    /// </summary>
    private static readonly List<string> _tempDirPaths = new();

    /// <summary>
    ///     反射读取 FormFileExtensions 内部目录缓存，用于断言规范化后缓存键的唯一性
    /// </summary>
    /// <returns>目录缓存字典</returns>
    private static ConcurrentDictionary<string, byte> ExistingDirCache()
    {
        var field = typeof(FormFileExtensions).GetField("ExistingDirCache",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (ConcurrentDictionary<string, byte>)field.GetValue(null)!;
    }

    /// <summary>
    ///     清理全部临时目录（xUnit 每用例结束调用），保证测试退出后不留文件系统残留
    /// </summary>
    public void Dispose()
    {
        foreach (var dir in _tempDirPaths)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
            catch (IOException)
            {
                // 并发测试遗留句柄未释放时忽略清理失败，避免影响测试结果
            }
        }
    }

    /// <summary>
    ///     空值校验：null 或纯空白路径必须抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_ThrowsOnNullOrWhitespace()
    {
        Assert.Throws<ArgumentNullException>(() => FormFileExtensions.EnsureDirectoryExistsCached(null!));
        Assert.Throws<ArgumentNullException>(() => FormFileExtensions.EnsureDirectoryExistsCached(string.Empty));
        Assert.Throws<ArgumentNullException>(() => FormFileExtensions.EnsureDirectoryExistsCached("   "));
    }

    /// <summary>
    ///     路径规范化：同一目录的不同写法（含冗余分隔符与 . 段）收敛为单一缓存键，目录只创建一次
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_NormalizesPathAndCachesOnce()
    {
        var dir = NewTempDirPath();
        // 两种写法规范化后指向同一完整路径：{dir}/sub 与 {dir}/./sub
        var variantA = Path.Combine(dir, "sub");
        var variantB = Path.Combine(dir, ".", "sub");
        var normalized = Path.GetFullPath(Path.Combine(dir, "sub"));

        FormFileExtensions.EnsureDirectoryExistsCached(variantA);
        FormFileExtensions.EnsureDirectoryExistsCached(variantB);

        Assert.True(Directory.Exists(normalized));
        // 缓存键唯一：该规范化路径在缓存中只出现一次（同一目录不同写法不再重复缓存）
        Assert.Single(ExistingDirCache().Keys, key => key == normalized);
    }

    /// <summary>
    ///     已存在目录：重复调用无异常、无副作用，并写入缓存
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_ExistingDirectoryIsIdempotentAndCached()
    {
        var dir = NewTempDirPath();
        Directory.CreateDirectory(dir);

        FormFileExtensions.EnsureDirectoryExistsCached(dir);
        FormFileExtensions.EnsureDirectoryExistsCached(dir);

        Assert.True(Directory.Exists(dir));
        Assert.Contains(Path.GetFullPath(dir), ExistingDirCache().Keys);
    }

    /// <summary>
    ///     Windows 错误码路径：CreateDirectory 抛 HResult 低 16 位为 183（ERROR_ALREADY_EXISTS）的 IOException 时，
    ///     视为目录已被他人创建而静默成功并缓存（模拟 Windows 跨进程并发竞态，且不依赖磁盘校验）
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_WindowsError183IsSwallowedAndCached()
    {
        var dir = NewTempDirPath();
        var originalDirectoryCreator = FormFileExtensions.DirectoryCreator;
        var originalIsWindows = FormFileExtensions.IsWindowsPlatform;
        // 0x800700B7：HRESULT 化后的 Windows 错误码 183（ERROR_ALREADY_EXISTS）；
        // 强制按 Windows 平台判定，使错误码 183 分支在非 Windows 环境亦可覆盖
        FormFileExtensions.DirectoryCreator = _ => throw new IOException("simulated: already exists")
        {
            HResult = unchecked((int)0x800700B7)
        };
        FormFileExtensions.IsWindowsPlatform = () => true;
        try
        {
            FormFileExtensions.EnsureDirectoryExistsCached(dir);

            // 错误码路径直接判定已存在：不抛异常且写入缓存（目录实际未创建，证明未走磁盘校验降级）
            Assert.Contains(Path.GetFullPath(dir), ExistingDirCache().Keys);
            Assert.False(Directory.Exists(dir));
        }
        finally
        {
            FormFileExtensions.DirectoryCreator = originalDirectoryCreator;
            FormFileExtensions.IsWindowsPlatform = originalIsWindows;
        }
    }

    /// <summary>
    ///     磁盘校验路径：CreateDirectory 抛非 183 IOException 但目录实际已存在时（并发下被他人创建），
    ///     通过磁盘校验复用并缓存，不抛异常
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_NonAlreadyExistsErrorWithExistingDirFallsBackToDiskCheck()
    {
        var dir = NewTempDirPath();
        Directory.CreateDirectory(dir);
        var originalDirectoryCreator = FormFileExtensions.DirectoryCreator;
        var originalIsWindows = FormFileExtensions.IsWindowsPlatform;
        // 非 183 错误码（此处为访问拒绝语义），模拟 Windows 下其他 IO 错误；强制按非 Windows 平台判定走磁盘校验
        FormFileExtensions.DirectoryCreator = _ => throw new IOException("simulated: access denied")
        {
            HResult = unchecked((int)0x80070005)
        };
        FormFileExtensions.IsWindowsPlatform = () => false;
        try
        {
            FormFileExtensions.EnsureDirectoryExistsCached(dir);

            Assert.True(Directory.Exists(dir));
            Assert.Contains(Path.GetFullPath(dir), ExistingDirCache().Keys);
        }
        finally
        {
            FormFileExtensions.DirectoryCreator = originalDirectoryCreator;
            FormFileExtensions.IsWindowsPlatform = originalIsWindows;
        }
    }

    /// <summary>
    ///     真实创建失败：CreateDirectory 抛非 183 IOException 且磁盘校验确认目录不存在时，
    ///     重新抛出包含完整路径的 IOException，且不污染缓存
    /// </summary>
    [Fact]
    public void EnsureDirectoryExistsCached_CreationFailureRethrowsWrappedWithPath()
    {
        var dir = NewTempDirPath();
        var originalDirectoryCreator = FormFileExtensions.DirectoryCreator;
        var originalIsWindows = FormFileExtensions.IsWindowsPlatform;
        var inner = new IOException("simulated: disk full") { HResult = unchecked((int)0x80070070) };
        FormFileExtensions.DirectoryCreator = _ => throw inner;
        FormFileExtensions.IsWindowsPlatform = () => false;
        try
        {
            var ex = Assert.Throws<IOException>(() => FormFileExtensions.EnsureDirectoryExistsCached(dir));

            Assert.Contains(Path.GetFullPath(dir), ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.DoesNotContain(Path.GetFullPath(dir), ExistingDirCache().Keys);
        }
        finally
        {
            FormFileExtensions.DirectoryCreator = originalDirectoryCreator;
            FormFileExtensions.IsWindowsPlatform = originalIsWindows;
        }
    }

    /// <summary>
    ///     并发创建：多个线程同时确保同一目录存在，全部成功且目录最终存在（缓存与 CreateDirectory 竞态均安全）
    /// </summary>
    [Fact]
    public async Task EnsureDirectoryExistsCached_ConcurrentCallsAllSucceed()
    {
        var dir = NewTempDirPath();
        const int count = 16;

        var tasks = Enumerable.Range(0, count)
            .Select(_ => Task.Run(() => FormFileExtensions.EnsureDirectoryExistsCached(dir)))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.True(Directory.Exists(dir));
    }
}
