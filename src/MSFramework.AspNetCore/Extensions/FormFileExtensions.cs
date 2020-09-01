using System;
using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions
{
	public static class FormFileExtensions
	{
		public static async Task<(string OriginName, string NewPath)> SaveAsync(this IFormFile formFile,
			string interval = "upload")
		{
#if NETSTANDARD2_1
			await using var stream = new MemoryStream();
#else
			using var stream = new MemoryStream();
#endif
			await formFile.CopyToAsync(stream);
			var bytes = stream.ToArray();

			var extension = Path.GetExtension(formFile.FileName);
			var md5 = bytes.ComputeMD5();
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
#if NETSTANDARD2_1
				await File.WriteAllBytesAsync(filePath, bytes);
#else
				File.WriteAllBytes(filePath, bytes);
#endif
			}

			return (formFile.FileName, $"{path}/{fileName}");
		}
	}
}