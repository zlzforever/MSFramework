using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///
/// </summary>
public static class FormFileExtensions
{
    private static readonly ConcurrentDictionary<string, bool> VirtualFolderState = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static async Task<SaveResult> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        var extension = Path.GetExtension(formFile.FileName);
        var date = $"{DateTime.Now:yyyMMdd}";
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
        var groupPath = Path.Combine(Defaults.OSSDirectory, level1, level2);
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
///
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SaveResult
{
    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string PhysicalPath { get; set; }
}
