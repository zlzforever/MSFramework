using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///     上传文件的存储扩展方法
/// </summary>
public static class FormFileExtensions
{
    private static readonly ConcurrentDictionary<string, bool> VirtualFolderState = new();

    /// <summary>
    /// 物理存于 /wwwroot/oss/{md5:0-1}/{md5:2-3}/{md5}.{extension}
    /// 虚拟存于 /wwwroot/upload/20260710/EED3EFA1750D0A147098D694ADE825B5.csv
    /// </summary>
    /// <param name="formFile">上传的文件</param>
    /// <param name="interval">虚拟目录间隔名</param>
    /// <returns>保存结果</returns>
    /// <exception cref="ArgumentNullException">formFile 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">interval 包含非法路径字符时抛出</exception>
    public static async Task<SaveResult> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        ArgumentNullException.ThrowIfNull(formFile);

        if (interval.Contains("..") || interval.Contains('/') || interval.Contains('\\'))
        {
            throw new ArgumentException("Invalid interval path");
        }

        var extension = Path.GetExtension(formFile.FileName);
        // 日期格式 yyyyMMdd，修正原 yyyMMdd 笔误
        var date = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var intervalDirectory = Path.Combine(interval, date);
        var virtualDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", intervalDirectory);
        EnsureDirectory(virtualDirectory);

        await using var stream = formFile.OpenReadStream();
        // 使用流式接口，增强性能
        var md5 = await Utils.Cryptography.ComputeMD5Async(stream);
        var level1 = md5.Substring(0, 2);
        var level2 = md5.Substring(2, 2);
        var fileName = $"{md5}{extension}";
        // upload/20251225/C4CA4238A0B923820DCC509A6F75849B.txt
        var virtualPath = Path.Combine(virtualDirectory, fileName);
        var intervalPath = Path.Combine(intervalDirectory, fileName);
        var groupPath = Path.Combine(Defaults.LocalOSSDirectory, level1, level2);
        var physicalPath = Path.Combine(groupPath, fileName);
        if (!File.Exists(virtualPath))
        {
            // 并发请求可能同时进入该分支，FileMode.Create 截断写入保证幂等
            EnsureDirectory(groupPath);
            await using (var outStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                if (stream.CanSeek)
                {
                    // MD5 计算已消费流内容，回到起始位置以便重新写入
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(outStream);
                }
                else
                {
                    // 非可寻址流无法回退位置，重新打开读取流（IFormFile 每次调用可重新打开）
                    // 以保证写出完整内容，而非 0 字节空文件
                    await using var reopenedStream = formFile.OpenReadStream();
                    await reopenedStream.CopyToAsync(outStream);
                }
            }

            await CreateLinkOrCopyAsync(virtualPath, physicalPath);
        }

        return new SaveResult { Name = formFile.FileName, Path = intervalPath, PhysicalPath = physicalPath };
    }

    /// <summary>
    /// 幂等创建目录（并发安全，重复调用无副作用）
    /// </summary>
    /// <param name="path">目录路径</param>
    private static void EnsureDirectory(string path)
    {
        VirtualFolderState.GetOrAdd(path, p =>
        {
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p!);
            }

            return true;
        });
    }

    /// <summary>
    /// 为虚拟路径创建指向物理文件的符号链接；
    /// 符号链接创建失败（文件系统不支持或无权限）时降级为文件复制并记录告警日志
    /// </summary>
    /// <param name="virtualPath">虚拟文件路径</param>
    /// <param name="physicalPath">物理文件路径</param>
    private static async Task CreateLinkOrCopyAsync(string virtualPath, string physicalPath)
    {
        if (File.Exists(virtualPath))
        {
            // 并发场景下其他请求已创建链接
            return;
        }

        try
        {
            File.CreateSymbolicLink(virtualPath, physicalPath);
        }
        catch (Exception ex)
        {
            Defaults.Logger?.LogWarning(ex,
                "创建符号链接失败，降级为文件复制: {VirtualPath} -> {PhysicalPath}", virtualPath, physicalPath);
            if (!File.Exists(virtualPath))
            {
                // 覆盖写保证幂等
                File.Copy(physicalPath, virtualPath, overwrite: true);
            }
        }
    }
}

/// <summary>
///     文件保存结果，包含原始名称、虚拟路径和物理路径
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SaveResult
{
    /// <summary>
    ///     文件原始名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件虚拟路径（相对路径）
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     文件物理磁盘路径
    /// </summary>
    public string PhysicalPath { get; set; }
}
