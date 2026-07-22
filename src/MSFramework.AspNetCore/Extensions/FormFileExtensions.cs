using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
    /// <param name="formFile"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static async Task<SaveResult> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        if (interval.Contains("..") || interval.Contains('/') || interval.Contains('\\'))
        {
            throw new ArgumentException("Invalid interval path");
        }
        var extension = Path.GetExtension(formFile.FileName);
        var date = $"{DateTimeOffset.UtcNow:yyyMMdd}";
        var intervalDirectory = Path.Combine(interval, date);
        var virtualDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", intervalDirectory);
        VirtualFolderState.GetOrAdd(virtualDirectory, path =>
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path!);
            }

            return true;
        });

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
            // wwwroot/oss/C4/CA/C4CA4238A0B923820DCC509A6F75849B.txt
            VirtualFolderState.GetOrAdd(groupPath, path =>
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path!);
                }

                return true;
            });
            await using (Stream outStream = File.OpenWrite(physicalPath))
            {
                await stream.CopyToAsync(outStream);
            }

            File.CreateSymbolicLink(virtualPath, physicalPath);
        }

        return new SaveResult { Name = formFile.FileName, Path = intervalPath, PhysicalPath = physicalPath };
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
