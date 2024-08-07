using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class HttpUtilTests
{
    [Fact]
    public void IsSuccessStatusCode_ReturnsTrue_ForStatusCode200()
    {
        var result = HttpUtil.IsSuccessStatusCode(200);
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsTrue_ForStatusCode299()
    {
        var result = HttpUtil.IsSuccessStatusCode(299);
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsFalse_ForStatusCode199()
    {
        var result = HttpUtil.IsSuccessStatusCode(199);
        Assert.False(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsFalse_ForStatusCode300()
    {
        var result = HttpUtil.IsSuccessStatusCode(300);
        Assert.False(result);
    }
}
