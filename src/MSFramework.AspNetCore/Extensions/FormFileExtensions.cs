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
    /// 符号链接创建委托（内部可测试性扩展点，默认使用 <see cref="File.CreateSymbolicLink(string, string)"/>；
    /// 测试可替换为抛异常委托以验证符号链接失败时的降级复制路径）
    /// </summary>
    internal static Func<string, string, FileSystemInfo> LinkCreator { get; set; } = File.CreateSymbolicLink;

    /// <summary>
    /// 存储结构（用户确认的新方案）：
    /// 真实文件存于 /wwwroot/upload/{date}/{md5前2位}/{md5次2位}/{md5}.{extension}（数据只写这里）
    /// 判重链接存于 /wwwroot/oss/{md5前2位}/{md5次2位}/{md5}.{extension}（符号链接指向真实文件，仅用于判重）
    /// 判重键为 oss 链接（内容指纹恒定、与日期无关），跨日期重复上传零数据写（仅新增 upload 链接）
    /// </summary>
    /// <param name="formFile">上传的文件</param>
    /// <param name="interval">虚拟目录间隔名（默认 upload）</param>
    /// <returns>保存结果：Path 为 upload 相对真实文件路径，PhysicalPath 为 oss 判重链接路径</returns>
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

        await using var stream = formFile.OpenReadStream();
        // 使用流式接口，增强性能
        var md5 = await Utils.Cryptography.ComputeMD5Async(stream);
        var level1 = md5.Substring(0, 2);
        var level2 = md5.Substring(2, 2);
        var fileName = $"{md5}{extension}";
        // 真实文件目标（md5 二级分层防单日目录爆炸）：upload/{date}/{l1}/{l2}/{md5}{ext}
        var intervalDirectory = Path.Combine(interval, date, level1, level2);
        var virtualDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", intervalDirectory);
        // upload/20251225/AB/CD/C4CA4238A0B923820DCC509A6F75849B.txt
        var intervalPath = Path.Combine(intervalDirectory, fileName);
        // Path.Combine 存在可空参数重载（返回 string?），此处入参均为非空字符串，
        // 结果不可能为 null；显式判空兜底以消除静态分析空值告警，并保证后续使用恒非空
        var virtualPath = Path.Combine(virtualDirectory, fileName)
            ?? throw new InvalidOperationException($"无法生成虚拟文件路径: {intervalPath}");
        // 判重链接目标（仅作判重用，指向 upload 真实文件）：oss/{l1}/{l2}/{md5}{ext}
        var groupPath = Path.Combine(Defaults.LocalOSSDirectory, level1, level2);
        var physicalPath = Path.Combine(groupPath, fileName);

        EnsureDirectory(virtualDirectory);

        if (!File.Exists(physicalPath))
        {
            // 首次上传（或并发竞态下链接尚未建立）：写入真实文件，FileMode.Create 截断写保证幂等，
            // FileShare.ReadWrite 允许并发写者同时打开，同内容并发写最终内容一致
            EnsureDirectory(groupPath);
            await using (var outStream = new FileStream(virtualPath, FileMode.Create, FileAccess.Write,
                             FileShare.ReadWrite))
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

            // 创建 oss 判重链接；并发场景下链接已被另一进程创建时，catch 中复用既有链接
            CreateLinkOrCopy(physicalPath, virtualPath);
        }
        else
        {
            // 判重命中：同内容已上传过（真实文件可能位于更早日期的 upload 目录），
            // 仅创建新的 upload 符号链接指向既有真实文件，零数据写
            var existingPath = File.ResolveLinkTarget(physicalPath, returnFinalTarget: true)?.FullName
                               ?? physicalPath;
            CreateLinkOrCopy(virtualPath, existingPath);
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
            // 覆盖写保证幂等
            File.Copy(targetPath, linkPath, overwrite: true);
        }
    }
}

/// <summary>
///     文件保存结果，包含原始名称、upload 虚拟路径和 oss 判重链接路径
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SaveResult
{
    /// <summary>
    ///     文件原始名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件虚拟路径（相对路径）：真实文件所在路径 upload/{date}/{l1}/{l2}/{md5}{ext}；
    ///     重复上传时为指向既有真实文件的符号链接路径，语义上均为可访问文件内容的路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     判重链接路径：oss/{l1}/{l2}/{md5}{ext}，为指向 upload 真实文件的符号链接（判重键，与日期无关）；
    ///     文件系统不支持符号链接时降级为真实文件的内容副本
    /// </summary>
    public string PhysicalPath { get; set; }
}
