using System;
using System.IO;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Utilities
{
	public class DirectoryUtilities
	{
		/// <summary>
		/// 创建文件夹，如果不存在
		/// </summary>
		/// <param name="directory">要创建的文件夹路径</param>
		public static void CreateIfNotExists(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}

		/// <summary>
		/// 递归复制文件夹及文件夹/文件
		/// </summary>
		/// <param name="sourcePath"> 源文件夹路径 </param>
		/// <param name="targetPath"> 目的文件夹路径 </param>
		/// <param name="searchPatterns"> 要复制的文件扩展名数组 </param>
		public static void Copy(string sourcePath, string targetPath, string[] searchPatterns = null)
		{
			Check.NotEmpty(sourcePath, nameof(sourcePath));
			Check.NotEmpty(targetPath, nameof(targetPath));

			if (!Directory.Exists(sourcePath))
			{
				throw new DirectoryNotFoundException("递归复制文件夹时源目录不存在。");
			}

			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}

			var dirs = Directory.GetDirectories(sourcePath);
			if (dirs.Length > 0)
			{
				foreach (var dir in dirs)
				{
					Copy(dir, targetPath + dir.Substring(dir.LastIndexOf("\\", StringComparison.Ordinal)));
				}
			}

			if (searchPatterns != null && searchPatterns.Length > 0)
			{
				foreach (var searchPattern in searchPatterns)
				{
					var files = Directory.GetFiles(sourcePath, searchPattern);
					if (files.Length <= 0)
					{
						continue;
					}

					foreach (var file in files)
					{
						File.Copy(file, targetPath + file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal)));
					}
				}
			}
			else
			{
				var files = Directory.GetFiles(sourcePath);
				if (files.Length <= 0)
				{
					return;
				}

				foreach (var file in files)
				{
					File.Copy(file, targetPath + file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal)));
				}
			}
		}

		/// <summary>
		/// 递归删除目录
		/// </summary>
		/// <param name="directory"> 目录路径 </param>
		/// <param name="isDeleteRoot"> 是否删除根目录 </param>
		/// <returns> 是否成功 </returns>
		public static bool Delete(string directory, bool isDeleteRoot = true)
		{
			Check.NotEmpty(directory, nameof(directory));
			var flag = false;
			var dirPathInfo = new DirectoryInfo(directory);
			if (dirPathInfo.Exists)
			{
				//删除目录下所有文件
				foreach (var fileInfo in dirPathInfo.GetFiles())
				{
					fileInfo.Delete();
				}

				//递归删除所有子目录
				foreach (var subDirectory in dirPathInfo.GetDirectories())
				{
					Delete(subDirectory.FullName);
				}

				//删除目录
				if (isDeleteRoot)
				{
					dirPathInfo.Attributes = FileAttributes.Normal;
					dirPathInfo.Delete();
				}

				flag = true;
			}

			return flag;
		}
	}
}