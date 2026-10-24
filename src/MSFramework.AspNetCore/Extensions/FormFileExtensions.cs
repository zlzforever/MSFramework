using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///     上传文件的存储扩展方法
/// </summary>
public static class FormFileExtensions
{
    /// <summary>
    ///     已确认存在的目录缓存：键为经 <see cref="Path.GetFullPath(string)"/> 规范化后的完整路径，值为占位标记；
    ///     使用线程安全容器承载，缓存命中时跳过重复的目录探测与创建（同一目录不同写法只占一个缓存键）
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte> ExistingDirCache = new ConcurrentDictionary<string, byte>();

    /// <summary>
    ///     符号链接创建委托（内部可测试性扩展点，默认使用 <see cref="File.CreateSymbolicLink(string, string)"/>；
    ///     测试可替换为抛异常委托以验证符号链接失败时的降级复制路径）
    /// </summary>
    internal static Func<string, string, FileSystemInfo> LinkCreator { get; set; } = File.CreateSymbolicLink;

    /// <summary>
    ///     目录创建委托（内部可测试性扩展点，默认使用 <see cref="Directory.CreateDirectory(string)"/>；
    ///     测试可替换为抛出指定 HResult 的 IOException 委托，以验证 Windows 错误码 183 与磁盘校验降级路径）
    /// </summary>
    internal static Func<string, DirectoryInfo> DirectoryCreator { get; set; } = Directory.CreateDirectory;

    /// <summary>
    ///     Windows 平台判定委托（内部可测试性扩展点，默认使用 <see cref="OperatingSystem.IsWindows"/>；
    ///     测试可替换为恒返回 true 的委托，以在非 Windows 环境验证错误码 183（ERROR_ALREADY_EXISTS）处理路径）
    /// </summary>
    internal static Func<bool> IsWindowsPlatform { get; set; } = OperatingSystem.IsWindows;

    /// <summary>
    /// 存储结构：
    /// 真实文件存于 Web 根目录之外的 {StorageRoot}/{interval}/{date}/{sha256前2位}/{sha256次2位}/{sha256}.{extension}。
    /// 判重链接存于 {DedupeRoot}/{sha256前2位}/{sha256次2位}/{sha256}.{extension}。
    /// 判重键为 oss 链接（内容指纹恒定、与日期无关），跨日期重复上传零数据写（仅新增 upload 链接）
    /// </summary>
    /// <param name="formFile">上传的文件</param>
    /// <param name="interval">业务目录间隔名（默认 upload）</param>
    /// <returns>保存结果：Path 为 upload 相对真实文件路径，PhysicalPath 为 oss 判重链接路径</returns>
    /// <exception cref="ArgumentNullException">formFile 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">interval 包含非法路径字符时抛出</exception>
    public static Task<SaveResult> SaveAsync(this IFormFile formFile, string interval = "upload")
    {
        return SaveAsync(formFile, new FileUploadOptions(), interval);
    }

    /// <summary>
    /// 使用显式安全策略保存上传文件。
    /// </summary>
    public static async Task<SaveResult> SaveAsync(this IFormFile formFile, FileUploadOptions options,
        string interval = "upload", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(formFile);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(interval) || interval.Contains("..") || interval.Contains('/') ||
            interval.Contains('\\'))
        {
            throw new ArgumentException("Invalid interval path");
        }

        var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
        if (!options.AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed.", nameof(formFile));
        }

        if (formFile.Length <= 0)
        {
            throw new ArgumentException("Empty files are not allowed.", nameof(formFile));
        }

        if (formFile.Length > options.MaxFileSize)
        {
            throw new ArgumentException($"File exceeds the {options.MaxFileSize} byte limit.", nameof(formFile));
        }

        var declaredContentType = formFile.Headers?.ContentType.ToString();
        if (!string.IsNullOrWhiteSpace(declaredContentType) &&
            (!options.AllowedContentTypes.TryGetValue(extension, out var contentTypes) ||
             !contentTypes.Contains(declaredContentType)))
        {
            throw new ArgumentException("The declared content type does not match the file extension.", nameof(formFile));
        }

        var date = DateTimeOffset.Now.ToString("yyyyMMdd");
        var stagingDirectory = Path.Combine(options.StorageRoot, ".staging");
        EnsureDirectoryExistsCached(stagingDirectory);
        var stagingPath = Path.Combine(stagingDirectory, $"{Guid.NewGuid():N}.upload");

        try
        {
            string digest;
            long bytesWritten = 0;
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var input = formFile.OpenReadStream();
            await using (var output = new FileStream(stagingPath, FileMode.CreateNew, FileAccess.Write,
                             FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    bytesWritten += read;
                    if (bytesWritten > options.MaxFileSize)
                    {
                        throw new ArgumentException($"File exceeds the {options.MaxFileSize} byte limit.", nameof(formFile));
                    }

                    hasher.AppendData(buffer, 0, read);
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                await output.FlushAsync(cancellationToken);
            }

            if (bytesWritten == 0)
            {
                throw new ArgumentException("Empty files are not allowed.", nameof(formFile));
            }

            digest = Convert.ToHexString(hasher.GetHashAndReset());
            var level1 = digest[..2];
            var level2 = digest.Substring(2, 2);
            var fileName = $"{digest}{extension}";
            var intervalDirectory = Path.Combine(interval, date, level1, level2);
            var businessDirectory = Path.Combine(options.StorageRoot, intervalDirectory);
            var businessPath = Path.Combine(businessDirectory, fileName);
            var intervalPath = Path.Combine(intervalDirectory, fileName);
            var dedupeLinkDirectory = Path.Combine(options.DedupeRoot, level1, level2);
            var dedupeLinkPath = Path.Combine(dedupeLinkDirectory, fileName);

            EnsureDirectoryExistsCached(businessDirectory);
            EnsureDirectoryExistsCached(dedupeLinkDirectory);

            if (!File.Exists(dedupeLinkPath))
            {
                try
                {
                    File.Move(stagingPath, businessPath, overwrite: false);
                }
                catch (IOException) when (File.Exists(businessPath))
                {
                    File.Delete(stagingPath);
                }

                CreateLinkOrCopy(dedupeLinkPath, businessPath);
            }
            else
            {
                File.Delete(stagingPath);
                var existingPath = File.ResolveLinkTarget(dedupeLinkPath, returnFinalTarget: true)?.FullName
                                   ?? dedupeLinkPath;
                CreateLinkOrCopy(businessPath, existingPath);
            }

            return new SaveResult { Name = formFile.FileName, Path = intervalPath, PhysicalPath = dedupeLinkPath };
        }
        finally
        {
            if (File.Exists(stagingPath))
            {
                File.Delete(stagingPath);
            }
        }
    }

    /// <summary>
    ///     幂等确保目录存在（并发安全）：路径先经 <see cref="Path.GetFullPath(string)"/> 规范化后作为缓存键，
    ///     缓存命中直接返回；创建目录时若因并发竞态抛出 IOException——
    ///     Windows 下优先按错误码 183（ERROR_ALREADY_EXISTS）判定目录已被他人创建，
    ///     非 Windows 下按磁盘实际状态（<see cref="Directory.Exists(string)"/>）校验——
    ///     均视为目录已存在并写入缓存后静默成功；仅当磁盘校验确认目录不存在时重新抛出
    /// </summary>
    /// <param name="path">待确保存在的目录路径；为 null 或纯空白时抛出 <see cref="ArgumentNullException"/></param>
    /// <exception cref="ArgumentNullException">path 为 null 或纯空白时抛出</exception>
    /// <exception cref="IOException">目录创建失败且磁盘校验确认目录不存在时抛出，异常消息包含完整路径</exception>
    internal static void EnsureDirectoryExistsCached(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        // 规范化完整路径，同一目录的不同写法（如冗余分隔符/./..）收敛为单一缓存键
        var fullPath = Path.GetFullPath(path);
        if (ExistingDirCache.ContainsKey(fullPath))
        {
            return;
        }

        try
        {
            DirectoryCreator(fullPath);
            ExistingDirCache.TryAdd(fullPath, 0);
        }
        catch (IOException ex)
        {
            // Windows 优先判断错误码 183（ERROR_ALREADY_EXISTS）：
            // 并发下另一进程已创建目录时 CreateDirectory 抛 IOException，错误码 183 即"目录已存在"
            var isAlreadyExistsError = IsWindowsPlatform() && (ex.HResult & 0xFFFF) == 183;
            if (isAlreadyExistsError)
            {
                ExistingDirCache.TryAdd(fullPath, 0);
                return;
            }

            // Linux / macOS / 其他 IO 错误：必须磁盘校验真实状态，
            // 目录已存在（并发竞态被他人创建）则复用，否则抛出
            if (Directory.Exists(fullPath))
            {
                ExistingDirCache.TryAdd(fullPath, 0);
                return;
            }

            throw new IOException($"创建目录失败 {fullPath}", ex);
        }
    }

    /// <summary>
    /// 为 linkPath 创建指向 targetPath 的符号链接（不做 File.Exists 预判断，直接创建——跨进程预判断不可靠）；
    /// 链接已存在（并发场景下另一进程已创建）时复用既有链接；
    /// 符号链接创建失败（文件系统不支持或无权限）时降级为文件复制并记录告警日志
    /// </summary>
    /// <param name="linkPath">链接文件路径（必须非空，调用方保证）</param>
    /// <param name="targetPath">链接指向的目标路径（必须非空，调用方保证）</param>
    private static void CreateLinkOrCopy(string linkPath, string targetPath)
    {
        try
        {
            LinkCreator(linkPath, targetPath);
        }
        catch (Exception ex)
        {
            if (File.Exists(linkPath))
            {
                // 并发场景下另一进程已创建链接（或同日期判重时 upload 路径已存在真实文件），直接复用
                return;
            }

            Defaults.Logger?.LogWarning(ex,
                "创建符号链接失败，降级为文件复制: {LinkPath} -> {TargetPath}", linkPath, targetPath);
            CopyAtomically(targetPath, linkPath);
        }
    }

    private static void CopyAtomically(string sourcePath, string destinationPath)
    {
        var directory = Path.GetDirectoryName(destinationPath)
                        ?? throw new ArgumentException("Destination must include a directory.", nameof(destinationPath));
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.Copy(sourcePath, temporaryPath, overwrite: false);
            try
            {
                File.Move(temporaryPath, destinationPath, overwrite: false);
            }
            catch (IOException) when (File.Exists(destinationPath))
            {
                // Another writer atomically published the same content first.
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}

/// <summary>
/// 上传文件的大小、类型与存储位置策略。
/// </summary>
public sealed class FileUploadOptions
{
    /// <summary>单个文件允许的最大字节数。</summary>
    public long MaxFileSize { get; init; } = 10 * 1024 * 1024;

    /// <summary>位于 Web 根目录之外的物理存储根目录。</summary>
    public string StorageRoot { get; init; } = Defaults.LocalStorageDirectory;

    /// <summary>内容寻址去重目录。</summary>
    public string DedupeRoot { get; init; } = Defaults.LocalOSSDirectory;

    /// <summary>允许的文件扩展名。</summary>
    public ISet<string> AllowedExtensions { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".csv", ".txt", ".json", ".xml", ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".zip"
    };

    /// <summary>扩展名对应的允许 MIME 类型。空 Content-Type 仍按扩展名策略处理。</summary>
    public IDictionary<string, ISet<string>> AllowedContentTypes { get; init; } =
        new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [".csv"] = new HashSet<string>(["text/csv", "application/csv", "text/plain"], StringComparer.OrdinalIgnoreCase),
            [".txt"] = new HashSet<string>(["text/plain"], StringComparer.OrdinalIgnoreCase),
            [".json"] = new HashSet<string>(["application/json", "text/json"], StringComparer.OrdinalIgnoreCase),
            [".xml"] = new HashSet<string>(["application/xml", "text/xml"], StringComparer.OrdinalIgnoreCase),
            [".pdf"] = new HashSet<string>(["application/pdf"], StringComparer.OrdinalIgnoreCase),
            [".png"] = new HashSet<string>(["image/png"], StringComparer.OrdinalIgnoreCase),
            [".jpg"] = new HashSet<string>(["image/jpeg"], StringComparer.OrdinalIgnoreCase),
            [".jpeg"] = new HashSet<string>(["image/jpeg"], StringComparer.OrdinalIgnoreCase),
            [".gif"] = new HashSet<string>(["image/gif"], StringComparer.OrdinalIgnoreCase),
            [".webp"] = new HashSet<string>(["image/webp"], StringComparer.OrdinalIgnoreCase),
            [".zip"] = new HashSet<string>(["application/zip", "application/x-zip-compressed"], StringComparer.OrdinalIgnoreCase)
        };
}

/// <summary>
///     文件保存结果，包含原始名称、业务相对路径和判重链接路径
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SaveResult
{
    /// <summary>
    ///     文件原始名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件业务路径（相对路径）：真实文件所在路径 {interval}/{date}/{l1}/{l2}/{sha256}{ext}；
    ///     重复上传时为指向既有真实文件的符号链接路径，语义上均为可访问文件内容的路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     判重链接路径：{DedupeRoot}/{l1}/{l2}/{sha256}{ext}，为指向真实文件的符号链接（判重键，与日期无关）；
    ///     文件系统不支持符号链接时降级为真实文件的内容副本
    /// </summary>
    public string PhysicalPath { get; set; }
}
