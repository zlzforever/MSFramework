using System.IO;
using System.Text.RegularExpressions;

namespace MicroserviceFramework.IO;

public static class DirectoryInfoExtensions
{
	public static void Copy(this DirectoryInfo dir, string destination, bool recursive,
		string exclude = null)
	{
		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
		}

		var dirs = dir.GetDirectories();
		Directory.CreateDirectory(destination);

		foreach (var file in dir.GetFiles())
		{
			if (!string.IsNullOrEmpty(exclude) && Regex.IsMatch(file.Name, exclude))
			{
				continue;
			}

			var targetFilePath = Path.Combine(destination, file.Name);
			file.CopyTo(targetFilePath);
		}

		if (!recursive)
		{
			return;
		}

		foreach (var subDir in dirs)
		{
			if (!string.IsNullOrEmpty(exclude) && Regex.IsMatch(subDir.Name, exclude))
			{
				continue;
			}

			var newDestinationDir = Path.Combine(destination, subDir.Name);
			Copy(subDir, newDestinationDir, true);
		}
	}
}