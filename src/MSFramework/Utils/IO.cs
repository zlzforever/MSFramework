using System.IO;
using System.Threading.Tasks;

namespace MicroserviceFramework.Utils;

// ReSharper disable once InconsistentNaming
/// <summary>
/// 文件与流操作工具类
/// </summary>
public static class IO
{
    /// <param name="stream"></param>
    extension(Stream stream)
    {
        /// <summary>
        /// 流保存到文件，目标文件已存在时截断重写
        /// </summary>
        /// <param name="path">目标文件路径</param>
        public void SaveToFile(string path)
        {
            using var fileStream = File.Open(path, FileMode.Create);
            stream.CopyTo(fileStream);
        }

        /// <summary>
        /// 将流当前位置到末尾的内容异步读取为 byte[]，不依赖 Seek 支持
        /// </summary>
        /// <returns>流内容字节数组</returns>
        public async Task<byte[]> ToArrayAsync()
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// 将流当前位置到末尾的内容读取为 byte[]，不依赖 Seek 支持
        /// </summary>
        /// <returns>流内容字节数组</returns>
        public byte[] ToArray()
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
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
