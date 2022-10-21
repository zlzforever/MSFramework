using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace MicroserviceFramework.AspNetCore.Extensions;

public static class FormFileExtensions
{
    public static async Task<(string OriginName, string NewPath)> SaveAsync(this IFormFile formFile,
        string interval = "upload")
    {
        var extension = Path.GetExtension(formFile.FileName);
        var fileName = $"{ObjectId.GenerateNewId()}{extension}";
        var date = $"{DateTime.Now:yyyMMdd}";
        var path = $"{interval}/{date}";
        var directory = Path.Combine(AppContext.BaseDirectory, $"wwwroot/{path}");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var filePath = Path.Combine(directory, fileName);

        await using var stream = new MemoryStream();
        await formFile.CopyToAsync(stream);
        var bytes = stream.ToArray();
        await File.WriteAllBytesAsync(filePath, bytes);

        return (formFile.FileName, $"{path}/{fileName}");
    }
}
