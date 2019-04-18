using System;
using System.Collections.Generic;
using System.IO;

namespace MSFramework.IO.Directory
{
	public class DirectoryHelper
	{
		public static void Move(string sourcePath, string destPath)
		{
			if (System.IO.Directory.Exists(sourcePath))
			{
				if (!System.IO.Directory.Exists(destPath))
				{
					//目标目录不存在则创建
					try
					{
						System.IO.Directory.CreateDirectory(destPath);
					}
					catch (Exception ex)
					{
						throw new Exception("Create dest directory failed：" + ex.Message);
					}
				}

				//获得源文件下所有文件
				List<string> files = new List<string>(System.IO.Directory.GetFiles(sourcePath));
				files.ForEach(c =>
				{
					string destFile = Path.Combine(new[] {destPath, Path.GetFileName(c)});
					//覆盖模式
					if (File.Exists(destFile))
					{
						File.Delete(destFile);
					}

					File.Move(c, destFile);
				});
				//获得源文件下所有目录文件
				List<string> folders = new List<string>(System.IO.Directory.GetDirectories(sourcePath));

				folders.ForEach(c =>
				{
					string destDir = Path.Combine(new [] {destPath, Path.GetFileName(c)});
					//采用递归的方法实现
					Move(c, destDir);
				});
			}
			else
			{
				throw new DirectoryNotFoundException("Source directory not exists");
			}
		}
	}
}