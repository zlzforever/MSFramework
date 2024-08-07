using System.IO;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class IOTests
{
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
}
