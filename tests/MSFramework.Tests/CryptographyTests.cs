using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class CryptographyTests
{
    [Fact]
    public async Task ComputeMD5()
    {
        var md51 = Cryptography.ComputeMD5("123");
        var data = "123"u8.ToArray();
        var md52 = Cryptography.ComputeMD5(data);
        var md53 = await Cryptography.ComputeMD5Async(new MemoryStream(data));
        Assert.Equal("202CB962AC59075B964B07152D234B70", md51);
        Assert.Equal("202CB962AC59075B964B07152D234B70", md52);
        Assert.Equal("202CB962AC59075B964B07152D234B70", md53);
    }
}
