using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///
/// </summary>
public static class FormFileExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static async Task<(string FileName, string Path)> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        var extension = Path.GetExtension(formFile.FileName);

        var date = $"{DateTime.Now:yyyMMdd}";
        var intervalDirectory = $"{interval}/{date}";
        var directory = Path.Combine(AppContext.BaseDirectory, $"wwwroot/{intervalDirectory}");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = formFile.OpenReadStream();
        var md5 = await Utils.Cryptography.ComputeMD5Async(stream);
        var fileName = $"{md5}{extension}";
        var relativePath = $"{intervalDirectory}/{fileName}";

        var absolutePath = Path.Combine(directory, fileName);
        if (File.Exists(absolutePath))
        {
            return (formFile.FileName, relativePath);
        }

        await using (Stream outStream = File.OpenWrite(absolutePath))
        {
            await stream.CopyToAsync(outStream);
        }

        return (formFile.FileName, relativePath);
    }
}
