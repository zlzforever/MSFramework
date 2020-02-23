using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Extensions
{
	public static class FormFileExtensions
	{
		public static async Task<string> SaveAsync(this IFormFile formFile, string interval = "upload")
		{
			using var stream = new MemoryStream();
			await formFile.CopyToAsync(stream);
			var bytes = stream.ToArray();

			var extension = Path.GetExtension(formFile.FileName);
			var md5 = bytes.ToMd5();
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
				File.WriteAllBytes(filePath, bytes);
			}

			return $"{path}/{fileName}";
		}
	}
}