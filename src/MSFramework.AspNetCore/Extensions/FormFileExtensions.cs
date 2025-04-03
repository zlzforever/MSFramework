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
    /// <param name="createDirectory">是否自动创建文件夹</param>
    /// <returns></returns>
    public static async Task<SaveFileInfo> SaveAsync(this IFormFile formFile,
        string interval = "upload", bool createDirectory = false)
    {
        var extension = Path.GetExtension(formFile.FileName);

        var date = $"{DateTime.Now:yyyMMdd}";
        var directory = Path.Combine(AppContext.BaseDirectory, "wwwroot", interval, date);

        // comments by lewis at 20250403 如果大量调用， 都判断目录是否存在， 会影响性能， 由业务方自己创建
        if (createDirectory && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = formFile.OpenReadStream();
        var md5 = await Utils.Cryptography.ComputeMD5Async(stream);
        var fileName = $"{md5}{extension}";
        var relativePath = Path.Combine(interval, date, fileName);

        var absolutePath = Path.Combine(directory, fileName);
        if (File.Exists(absolutePath))
        {
            return new SaveFileInfo { Name = formFile.FileName, Path = relativePath };
        }

        await using (Stream outStream = File.OpenWrite(absolutePath))
        {
            await stream.CopyToAsync(outStream);
        }

        return new SaveFileInfo { Name = formFile.FileName, Path = relativePath };
    }
}
