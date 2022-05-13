using System;
using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.Utilities;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions
{
	public static class FormFileExtensions
	{
		public static async Task<(string OriginName, string NewPath)> SaveAsync(this IFormFile formFile,
			string interval = "upload")
		{
			await using var stream = new MemoryStream();
			await formFile.CopyToAsync(stream);
			var bytes = stream.ToArray();

			var extension = Path.GetExtension(formFile.FileName);
			var md5 = CryptographyUtilities.ComputeMD5(bytes);
			var fileName = $"{md5}{extension}";
			var date = $"{DateTime.Now:yyyMMdd}";
			var path = $"{interval}/{date}";
			var directory = Path.Combine(AppContext.BaseDirectory, $"wwwroot/{path}");
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var filePath = Path.Combine(directory, fileName);

			if (!File.Exists(filePath))
			{
				await File.WriteAllBytesAsync(filePath, bytes);
			}

			return (formFile.FileName, $"{path}/{fileName}");
		}
	}
}