using System.IO;
using System.Threading.Tasks;

namespace MicroserviceFramework.Utils;

// ReSharper disable once InconsistentNaming
/// <summary>
///
/// </summary>
public static class IO
{
    /// <summary>
    /// 流保存到文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="path"></param>
    public static void SaveToFile(this Stream stream, string path)
    {
        using var fileStream = File.Open(path, FileMode.OpenOrCreate);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// 流转换成 byte[]
    /// </summary>
    /// <param name="stream"></param>
    public static async Task<byte[]> ToArrayAsync(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        _ = await stream.ReadAsync(bytes);
        return bytes;
    }

    /// <summary>
    /// 流转换成 byte[]
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ToArray(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        _ = stream.Read(bytes);
        return bytes;
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
    /// <param name="source">源文件夹</param>
    /// <param name="destination">目标文件夹</param>
    /// <param name="recursive">是否递归复制</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(string source, string destination, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(source);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"源文件夹不存在: {dir.FullName}");
        }

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
#pragma warning disable RS1035
        Directory.CreateDirectory(destination);
#pragma warning restore RS1035

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destination, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destination, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
