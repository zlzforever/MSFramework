using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test;

public class ApiResultTests : BaseTest
{
    private readonly ITestOutputHelper _output;

    public ApiResultTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ReturnInt()
    {
        var result = await Client.GetStringAsync("/apiResult/int");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":7896}
""", result);
    }

    [Fact]
    public async Task ReturnString()
    {
        var result = await Client.GetStringAsync("/apiResult/string");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":"AAABBB"}
""", result);
    }

    [Fact]
    public async Task ReturnDatetime()
    {
        var result1 = await Client.GetStringAsync("/apiResult/dateTime");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":1689261960}
""", result1);

        var result2 = await Client.GetStringAsync("/apiResult/nullableDateTime1");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":null}
""", result2);

        var result3 = await Client.GetStringAsync("/apiResult/nullableDateTime2");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":1689261960}
""", result3);
    }

    [Fact]
    public async Task ReturnDateTimeOffset()
    {
        var result1 = await Client.GetStringAsync("/apiResult/dateTimeOffset");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":1689261960}
""", result1);

        var result2 = await Client.GetStringAsync("/apiResult/nullableDateTimeOffset1");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":null}
""", result2);

        var result3 = await Client.GetStringAsync("/apiResult/nullableDateTimeOffset2");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":1689261960}
""", result3);
    }

    [Fact]
    public async Task ReturnOk()
    {
        var result1 = await Client.GetStringAsync("/apiResult/ok");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":null}
""", result1);
    }

    [Fact]
    public async Task ReturnError()
    {
        var result1 = await Client.GetStringAsync("/apiResult/error");
        Assert.Equal("""
{"success":false,"code":1,"msg":"服务器内部错误","data":null}
""", result1);
    }

    [Fact]
    public async Task ReturnNoResponse()
    {
        var result1 = await Client.GetStringAsync("/apiResult/noResponse");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":null}
""", result1);
    }

    [Fact]
    public async Task ReturnList()
    {
        var result1 = await Client.GetStringAsync("/apiResult/list1");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":[1,2]}
""", result1);

        var result2 = await Client.GetStringAsync("/apiResult/list2");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":[1,2,3]}
""", result2);
    }

    [Fact]
    public async Task ReturnFile()
    {
        var result1 = await Client.GetStringAsync("/apiResult/file");
        Assert.Equal("""
c1,c2,c3
1,2,3
""", result1);
    }

    [Fact]
    public async Task ReturnObjectResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/objectResult1");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":1}
""", result1);

        var result2 = await Client.GetStringAsync("/apiResult/objectResult2");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":{"a":1,"b":2}}
""", result2);
    }

    [Fact]
    public async Task ReturnPagedResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/pagedResult");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":{"data":[1,2,3],"total":10,"page":1,"limit":10}}
""", result1);
    }


    [Fact]
    public async Task ReturnEmptyResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/emptyResult");
        Assert.Equal("""
{"success":true,"code":0,"msg":"","data":null}
""", result1);
    }
}
