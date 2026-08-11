using System;
using System.IO;
using System.Linq;
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
public class FormFileTests : BaseTest
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
}
