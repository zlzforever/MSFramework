using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class HttpUtilsTests
{
    [Fact]
    public void IsSuccessStatusCode_ReturnsTrue_ForStatusCode200()
    {
        var result = HttpUtils.IsSuccessStatusCode(200);
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsTrue_ForStatusCode299()
    {
        var result = HttpUtils.IsSuccessStatusCode(299);
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsFalse_ForStatusCode199()
    {
        var result = HttpUtils.IsSuccessStatusCode(199);
        Assert.False(result);
    }

    [Fact]
    public void IsSuccessStatusCode_ReturnsFalse_ForStatusCode300()
    {
        var result = HttpUtils.IsSuccessStatusCode(300);
        Assert.False(result);
    }
}
