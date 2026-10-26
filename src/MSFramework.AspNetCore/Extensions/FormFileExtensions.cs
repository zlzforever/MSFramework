using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// <summary>
    ///     已确认存在的目录缓存：键为经 <see cref="Path.GetFullPath(string)"/> 规范化后的完整路径，值为占位标记；
    ///     使用线程安全容器承载，缓存命中时跳过重复的目录探测与创建（同一目录不同写法只占一个缓存键）
    /// </summary>
    private static readonly ConcurrentDictionary<string, byte> ExistingDirCache = new ConcurrentDictionary<string, byte>();

    /// <summary>
    ///     单文件大小上限（100MB）：网关/Kestrel/Dapr 层已限制请求体大小，此处为存储层兜底
    /// </summary>
    public const long MaxFileLength = 100 * 1024 * 1024;

    // 禁止的危险后缀（补充拦截，双重保险）。
    // 注意：黑名单不是安全边界——只是降低误配置风险的兜底，存储目录应位于 webroot 之外或
    // 由静态文件中间件限制可解释的 Content-Type
    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".xhtml", ".shtml", ".js", ".mjs", ".json",
        ".php", ".php3", ".php4", ".php5", ".php7", ".php8", ".phtml", ".phps", ".phar",
        ".aspx", ".ashx", ".cshtml", ".asmx", ".ascx", ".master", ".config", ".webconfig", ".svc",
        ".asp", ".jsp", ".jspx", ".jspf",
        ".cfm", ".cfml", ".pl", ".cgi", ".perl", ".py", ".pyc", ".pyw", ".rb", ".rhtml", ".lua", ".go",
        ".svg", ".wasm",
        ".exe", ".com", ".scr", ".msi", ".bat", ".cmd", ".ps1", ".psm1", ".ps1xml",
        ".sh", ".ksh", ".zsh", ".csh", ".bash", ".elf", ".so", ".o", ".bin", ".dll", ".sys",
        ".htaccess", ".user.ini",
        ".jar", ".war", ".ear",
        ".sql", ".bak", ".dump", ".mdb", ".accdb", ".sqlite", ".db",
        ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso",
        ".xml"
    };

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
    /// 存储结构（用户确认的新方案）：
    /// 真实文件存于 {storage根}/upload/{date}/{md5前2位}/{md5次2位}/{md5}.{extension}（数据只写这里）
    /// 判重链接存于 {storage根}/oss/{md5前2位}/{md5次2位}/{md5}.{extension}（符号链接指向真实文件，仅用于判重）
    /// 判重键为 oss 链接（内容指纹恒定、与日期无关），跨日期重复上传零数据写（仅新增 upload 链接）。
    /// 存储根为 <see cref="Defaults.LocalStorageRoot"/>（webroot 之外），防止静态文件中间件未经授权外发上传内容
    /// </summary>
    /// <param name="formFile">上传的文件</param>
    /// <param name="interval">业务目录间隔名（默认 upload）</param>
    /// <returns>保存结果：Path 为 upload 相对真实文件路径，PhysicalPath 为 oss 判重链接相对路径（相对存储根）</returns>
    /// <exception cref="ArgumentNullException">formFile 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">interval 为空白、包含非法路径字符时抛出；
    /// 扩展名命中危险后缀黑名单时抛出</exception>
    /// <exception cref="InvalidDataException">文件为空或超过 <see cref="MaxFileLength"/> 时抛出</exception>
    public static async Task<SaveResult> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        ArgumentNullException.ThrowIfNull(formFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(interval);

        // 已知限制（勿在后续 review 中重复提出）：本校验只拦截 ../、/、\\，
        // 未覆盖 Windows 盘符相对路径（如 "C:upload" 经 Path.Combine 会脱离存储根）与
        // 保留设备名（CON/NUL 等）——本项目不面向 Windows Server 部署（Linux 容器），
        // 该限制可接受；若未来需要面向 Windows 宿主，请改为白名单校验
        // （仅允许 ASCII 字母/数字/'-'/'_'）
        if (interval.Contains("..") || interval.Contains('/') || interval.Contains('\\'))
        {
            throw new ArgumentException("Invalid interval path", nameof(interval));
        }

        // 空文件与大小上限：上游（网关/Kestrel/Dapr）已限制请求体大小，此处为存储层兜底
        if (formFile.Length is <= 0 or > MaxFileLength)
        {
            throw new InvalidDataException(
                $"文件大小 {formFile.Length} 超出允许范围 (0, {MaxFileLength}]");
        }

        var extension = Path.GetExtension(formFile.FileName);
        if (DangerousExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed for security reasons.");
        }

        // 日期格式 yyyyMMdd，修正原 yyyMMdd 笔误
        var date = DateTimeOffset.Now.ToString("yyyyMMdd");

        await using var stream = formFile.OpenReadStream();
        // 使用流式接口，增强性能
        var md5 = await Utils.Cryptography.ComputeMD5Async(stream);
        var level1 = md5.Substring(0, 2);
        var level2 = md5.Substring(2, 2);
        var fileName = $"{md5}{extension}";
        // 真实文件目标（md5 二级分层防单日目录爆炸）：storage/upload/{date}/{l1}/{l2}/{md5}{ext}；
        // 存储根位于 webroot 之外（Defaults.LocalStorageRoot），避免静态文件中间件直接外发上传内容
        var intervalDirectory = Path.Combine(interval, date, level1, level2);
        var businessDirectory = Path.Combine(Defaults.LocalStorageRoot, intervalDirectory);
        // upload/20251225/AB/CD/C4CA4238A0B923820DCC509A6F75849B.txt
        var intervalPath = Path.Combine(intervalDirectory, fileName);
        // Path.Combine 存在可空参数重载（返回 string?），此处入参均为非空字符串，
        // 结果不可能为 null；显式判空兜底以消除静态分析空值告警，并保证后续使用恒非空
        var businessPath = Path.Combine(businessDirectory, fileName);
        // 判重链接目标（仅作判重用，指向 upload 业务文件）：oss/{l1}/{l2}/{md5}{ext}
        var dedupeLinkDirectory = Path.Combine(Defaults.LocalOSSDirectory, level1, level2);
        var dedupeLinkPath = Path.Combine(dedupeLinkDirectory, fileName);

        EnsureDirectoryExistsCached(businessDirectory);

        if (!File.Exists(dedupeLinkPath))
        {
            // 首次上传（或并发竞态下链接尚未建立）：
            // 先写唯一临时文件（FileMode.CreateNew + FileShare.None 独占，杜绝并发读到未完成内容），
            // 再以同卷 Move 原子落到目标位置——目标已存在（并发竞态）时复用既有文件
            EnsureDirectoryExistsCached(dedupeLinkDirectory);
            var tempPath = Path.Combine(businessDirectory, $".{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var outStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write,
                                 FileShare.None))
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

                MoveOrReuse(tempPath, businessPath);
            }
            finally
            {
                // Move 成功或复用既有文件时 temp 已不存在；仅写失败时清理临时文件
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            // 创建 oss 判重链接；并发场景下链接已被另一进程创建时，catch 中复用既有链接
            CreateLinkOrCopy(dedupeLinkPath, businessPath);
        }
        else
        {
            // 判重命中：同内容已上传过（真实文件可能位于更早日期的 upload 目录），
            // 仅创建新的 upload 符号链接指向既有真实文件，零数据写
            var existingPath = File.ResolveLinkTarget(dedupeLinkPath, returnFinalTarget: true)?.FullName
                               ?? dedupeLinkPath;
            CreateLinkOrCopy(businessPath, existingPath);
        }

        // 返回相对路径：判重链接相对存储根 oss/{l1}/{l2}/{md5}{ext}，
        // 不回传服务器物理绝对路径，避免向客户端泄露部署根目录与目录布局
        var dedupeLinkRelativePath = Path.Combine("oss", level1, level2, fileName);

        return new SaveResult { Name = formFile.FileName, Path = intervalPath, PhysicalPath = dedupeLinkRelativePath };
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
            // 覆盖写保证幂等：先写唯一临时副本再原子替换（Move overwrite），
            // 避免进程中断/并发时在目标位置留下半成品
            var copyTempPath = Path.Combine(Path.GetDirectoryName(linkPath)!, $".{Guid.NewGuid():N}.tmp");
            try
            {
                File.Copy(targetPath, copyTempPath, overwrite: false);
                File.Move(copyTempPath, linkPath, overwrite: true);
            }
            finally
            {
                // Move 成功时临时文件已不存在；仅复制失败时清理
                if (File.Exists(copyTempPath))
                {
                    File.Delete(copyTempPath);
                }
            }
        }
    }

    /// <summary>
    ///     将临时文件原子移动到目标位置（同卷 Move 原子可见）；
    ///     目标已存在（并发竞态下另一进程先完成，同 MD5 内容一致）时复用既有文件，临时文件由调用方清理
    /// </summary>
    /// <param name="tempPath">已写入完成的临时文件路径</param>
    /// <param name="targetPath">目标位置</param>
    /// <exception cref="IOException">目标不存在且 Move 失败时抛出</exception>
    private static void MoveOrReuse(string tempPath, string targetPath)
    {
        try
        {
            File.Move(tempPath, targetPath);
        }
        catch (IOException)
        {
            if (File.Exists(targetPath))
            {
                // 并发竞态：目标已被其它进程先写入（同 MD5 内容一致），复用之
                return;
            }

            throw;
        }
    }
}

/// <summary>
///     文件保存结果，包含原始名称、upload 业务相对路径和 oss 判重链接相对路径
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SaveResult
{
    /// <summary>
    ///     文件原始名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     文件业务路径（相对路径，相对本地存储根 <see cref="Defaults.LocalStorageRoot"/>）：
    ///     真实文件所在路径 upload/{date}/{l1}/{l2}/{md5}{ext}；
    ///     重复上传时为指向既有真实文件的符号链接路径，语义上均为可访问文件内容的路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     判重链接路径（相对路径，相对本地存储根 <see cref="Defaults.LocalStorageRoot"/>）：
    ///     oss/{l1}/{l2}/{md5}{ext}，为指向 upload 真实文件的符号链接（判重键，与日期无关）；
    ///     文件系统不支持符号链接时降级为真实文件的内容副本。
    ///     仅回传相对路径，不包含服务器物理绝对路径，避免向客户端泄露部署目录结构
    /// </summary>
    public string PhysicalPath { get; set; }
}
