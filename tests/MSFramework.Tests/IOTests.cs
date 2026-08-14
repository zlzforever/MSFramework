using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class IOTests
{
    /// <summary>
    /// 不支持 Seek 的测试流，用于验证 IO 扩展方法不再依赖 Seek
    /// </summary>
    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    [Fact]
    public void CopyDirectory_CreatesDestinationDirectory()
    {
        var sourceDir = "sourceDir";
        var destinationDir = "destinationDir";
        Directory.CreateDirectory(sourceDir);
        IO.CopyDirectory(sourceDir, destinationDir, false);
        Assert.True(Directory.Exists(destinationDir));
        Directory.Delete(sourceDir);
        Directory.Delete(destinationDir);
    }

    [Fact]
    public void CopyDirectory_CopiesFiles()
    {
        var sourceDir = "sourceDir";
        var destinationDir = "destinationDir";
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "content");
        IO.CopyDirectory(sourceDir, destinationDir, false);
        Assert.True(File.Exists(Path.Combine(destinationDir, "test.txt")));
        Directory.Delete(sourceDir, true);
        Directory.Delete(destinationDir, true);
    }

    [Fact]
    public void CopyDirectory_ThrowsException_WhenSourceDirectoryDoesNotExist()
    {
        var sourceDir = "nonExistentDir";
        var destinationDir = "destinationDir";
        var exception =
            Assert.Throws<DirectoryNotFoundException>(() => IO.CopyDirectory(sourceDir, destinationDir, false));
        Assert.Equal($"源文件夹不存在: {Path.GetFullPath(sourceDir)}", exception.Message);
    }

    [Fact]
    public void CopyDirectory_CopiesSubdirectories_WhenRecursiveIsTrue()
    {
        var sourceDir = "sourceDir";
        var destinationDir = "destinationDir";
        var subDir = Path.Combine(sourceDir, "subDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "test.txt"), "content");
        IO.CopyDirectory(sourceDir, destinationDir, true);
        Assert.True(Directory.Exists(Path.Combine(destinationDir, "subDir")));
        Assert.True(File.Exists(Path.Combine(destinationDir, "subDir", "test.txt")));
        Directory.Delete(sourceDir, true);
        Directory.Delete(destinationDir, true);
    }

    [Fact]
    public void CopyDirectory_DoesNotCopySubdirectories_WhenRecursiveIsFalse()
    {
        var sourceDir = "sourceDir";
        var destinationDir = "destinationDir";
        var subDir = Path.Combine(sourceDir, "subDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "test.txt"), "content");
        IO.CopyDirectory(sourceDir, destinationDir, false);
        Assert.False(Directory.Exists(Path.Combine(destinationDir, "subDir")));
        Directory.Delete(sourceDir, true);
        Directory.Delete(destinationDir, true);
    }

    [Fact]
    public void ToArray_ReadsWholeStream_WithoutSeek()
    {
        // 旧实现依赖 Seek + Length，不支持 Seek 的流（如网络流）会抛异常；新实现复制到 MemoryStream
        using var stream = new NonSeekableStream(new MemoryStream(Encoding.UTF8.GetBytes("hello world")));

        var result = stream.ToArray();

        Assert.Equal("hello world", Encoding.UTF8.GetString(result));
    }

    [Fact]
    public async Task ToArrayAsync_ReadsWholeStream_WithoutSeek()
    {
        using var stream = new NonSeekableStream(new MemoryStream(Encoding.UTF8.GetBytes("hello world")));

        var result = await stream.ToArrayAsync();

        Assert.Equal("hello world", Encoding.UTF8.GetString(result));
    }

    [Fact]
    public void SaveToFile_TruncatesExistingFile()
    {
        // 旧实现 FileMode.OpenOrCreate 不截断，旧内容残留；新实现 FileMode.Create 截断重写
        var path = "saveToFileTest.txt";
        try
        {
            File.WriteAllText(path, "old content that must be truncated");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("new content"));

            stream.SaveToFile(path);

            Assert.Equal("new content", File.ReadAllText(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
