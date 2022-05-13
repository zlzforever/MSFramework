using System;
using System.IO;
using System.Text.RegularExpressions;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Utilities
{
	public static class DirectoryUtilities
	{
		public static void Copy(DirectoryInfo source, DirectoryInfo target, string excludePattern)
		{
			if (target.FullName.Contains(source.FullName))
			{
				throw new Exception("Cannot perform DeepCopy: Ancestry conflict detected");
			}

			// Go through the Directories and recursively call the DeepCopy Method for each one
			foreach (var dir in source.GetDirectories())
			{
				var match = Regex.Match(dir.Name, excludePattern);

				if (!match.Success)
				{
					Copy(dir, target.CreateSubdirectory(dir.Name), excludePattern);
				}
			}

			// Go ahead and copy each file to the target directory
			foreach (var file in source.GetFiles())
			{
				var match = Regex.Match(file.Name, excludePattern);

				if (!match.Success)
				{
					file.CopyTo(Path.Combine(target.FullName, file.Name));
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
			Check.NotNullOrWhiteSpace(directory, nameof(directory));
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