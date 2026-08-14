using MicroserviceFramework.Common;
using Xunit;

namespace MSFramework.Tests;

public class ApiResultTests
{
    [Fact]
    public void Ok_ReturnsNewInstance_EachAccess()
    {
        // 旧实现为共享可变静态实例，调用方篡改会污染其他请求；新实现每次访问返回新实例
        var first = ApiResult.Ok;
        var second = ApiResult.Ok;

        Assert.NotSame(first, second);
        Assert.True(first.Success);
        Assert.Equal(0, first.Code);
        Assert.Null(first.Data);
    }

    [Fact]
    public void Ok_MutatingInstance_DoesNotAffectOtherAccesses()
    {
        var first = ApiResult.Ok;
        first.Msg = "被篡改";

        Assert.Equal(string.Empty, ApiResult.Ok.Msg);
        Assert.NotSame(first, ApiResult.Ok);
    }

    [Fact]
    public void Error_ReturnsNewInstance_EachAccess()
    {
        var first = ApiResult.Error;
        var second = ApiResult.Error;

        Assert.NotSame(first, second);
        Assert.False(first.Success);
        Assert.Equal(1, first.Code);
        Assert.Equal("服务器内部错误", first.Msg);
    }

    [Fact]
    public void Error_MutatingInstance_DoesNotAffectOtherAccesses()
    {
        var first = ApiResult.Error;
        first.Msg = "被篡改";

        Assert.Equal("服务器内部错误", ApiResult.Error.Msg);
        Assert.NotSame(first, ApiResult.Error);
    }
}
