using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.AspNetCore.Test;

public class ApiResultTests(ITestOutputHelper output) : BaseTest
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task ValidationEnum()
    {
        var json = """
                   {
                     "state": "Ok"
                   }
                   """;
        var result1 = await Client
            .PostAsync("/apiResult/enum", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        var text1 = await result1.Content.ReadAsStringAsync();
        Assert.Equal(200, (int)result1.StatusCode);


        var result2 = await Client
            .PostAsync("/apiResult/enum", new StringContent("""
                                                            {
                                                              "state": "Error"
                                                            }
                                                            """, System.Text.Encoding.UTF8, "application/json"));
        var text2 = await result2.Content.ReadAsStringAsync();
        Assert.Equal(200, (int)result2.StatusCode);
    }

    [Fact]
    public async Task Validation()
    {
        var result = await Client.PostAsync("/apiResult/validation", new StringContent(""));
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(400, (int)result.StatusCode);
        Assert.Equal("""
                     {"errors":[{"name":"id","messages":["The id field is required."]}],"success":false,"code":1,"msg":"数据校验不通过","data":null}
                     """, text);
    }

    [Fact]
    public async Task InvalidObjectId_ReturnsHttp400AndModelStateError()
    {
        var result = await Client.GetAsync("/apiResult/objectId?id=not-an-object-id");
        var text = await result.Content.ReadAsStringAsync();

        Assert.Equal(400, (int)result.StatusCode);
        Assert.Contains("\"errors\"", text);
        Assert.Contains("\"id\"", text);
    }


    [Fact]
    public async Task Return452()
    {
        var result = await Client.GetAsync("/apiResult/452");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(452, (int)result.StatusCode);
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal(500, root.GetProperty("code").GetInt32());
        Assert.Equal(string.Empty, root.GetProperty("msg").GetString());
        Assert.Equal(452, root.GetProperty("data").GetProperty("status").GetInt32());
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
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "apiResult/string"));
        var str = await response.Content.ReadAsStringAsync();
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":"AAABBB"}
                     """, str);
    }

    [Fact]
    public async Task ReturnDatetime()
    {
        var t1 = new DateTime(2023, 07, 13, 23,
            26, 0);
        var result1 = await Client.GetStringAsync("/apiResult/dateTime");
        Assert.Equal($$"""
                       {"success":true,"code":0,"msg":"","data":{{t1.ToUnixTimeSeconds()}}}
                       """, result1);

        var result2 = await Client.GetStringAsync("/apiResult/nullableDateTime1");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":null}
                     """, result2);

        var t3 = new DateTime(2023, 07, 13, 23,
            26, 0);
        var result3 = await Client.GetStringAsync("/apiResult/nullableDateTime2");
        Assert.Equal($$"""
                       {"success":true,"code":0,"msg":"","data":{{t3.ToUnixTimeSeconds()}}}
                       """, result3);
    }

    [Fact]
    public async Task ReturnDateTimeOffset()
    {
        var t = new DateTimeOffset(2023, 07, 13, 23,
            26, 0, DateTimeOffset.Now.Offset);
        var result1 = await Client.GetStringAsync("/apiResult/dateTimeOffset");
        Assert.Equal($$"""
                       {"success":true,"code":0,"msg":"","data":{{t.ToUnixTimeSeconds()}}}
                       """, result1);

        var result2 = await Client.GetStringAsync("/apiResult/nullableDateTimeOffset1");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":null}
                     """, result2);

        var t3 = new DateTimeOffset(2023, 07, 13, 23,
            26, 0, DateTimeOffset.Now.Offset);
        var result3 = await Client.GetStringAsync("/apiResult/nullableDateTimeOffset2");
        Assert.Equal($$"""
                       {"success":true,"code":0,"msg":"","data":{{t3.ToUnixTimeSeconds()}}}
                       """, result3);
    }

//     [Fact]
//     public async Task ReturnOk()
//     {
//         var result1 = await Client.GetStringAsync("/apiResult/ok");
//         Assert.Equal("""
// {"success":true,"code":0,"msg":"","data":null}
// """, result1);
//     }

//     [Fact]
//     public async Task ReturnError()
//     {
//         var result1 = await Client.GetStringAsync("/apiResult/error");
//         Assert.Equal("""
// {"success":false,"code":1,"msg":"服务器内部错误","data":null}
// """, result1);
//     }

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

//         var result2 = await Client.GetStringAsync("/apiResult/list2");
//         Assert.Equal("""
// {"success":true,"code":0,"msg":"","data":[1,2,3]}
// """, result2);
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

    [Fact]
    public async Task ReturnProblemDetailsAsApiResultWithJsonContentTypeAsync()
    {
        var result = await Client.GetAsync("/apiResult/problemDetails");
        var text = await result.Content.ReadAsStringAsync();

        Assert.Equal(400, (int)result.StatusCode);
        Assert.Equal("application/json", result.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal(500, root.GetProperty("code").GetInt32());
        Assert.Equal("请求无效", root.GetProperty("msg").GetString());
        var data = root.GetProperty("data");
        Assert.Equal("请求无效", data.GetProperty("title").GetString());
        Assert.Equal(400, data.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task GetApiResultPreservesHttpContractAsync()
    {
        var result = await Client.GetAsync("/apiResult/apiResult");
        var result1 = await result.Content.ReadAsStringAsync();
        Assert.Equal(200, (int)result.StatusCode);
        Assert.Equal("application/json", result.Content.Headers.ContentType?.MediaType);
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":1}
                     """, result1);
    }

    [Fact]
    public async Task GetNewtonsoftJsonApiResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/newtonsoftJson");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":"{\"A\":\"he\",\"b\":12}"}
                     """, result1);
    }

    [Fact]
    public async Task GetApiResultGenericPreservesHttpContractAsync()
    {
        var result = await Client.GetAsync("/apiResult/apiResultGeneric");
        var result1 = await result.Content.ReadAsStringAsync();
        Assert.Equal(200, (int)result.StatusCode);
        Assert.Equal("application/json", result.Content.Headers.ContentType?.MediaType);
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":1}
                     """, result1);
    }

    [Theory]
    [InlineData("ordinaryBadRequest", 400, "普通请求错误")]
    [InlineData("ordinaryServerError", 500, "{\"success\":false,\"code\":1,\"msg\":\"\",\"data\":\"普通服务器错误\"}")]
    public async Task ReturnOrdinaryErrorAsApiResultWithHttpContractAsync(
        string route, int expectedStatusCode, string expectedData)
    {
        var result = await Client.GetAsync($"/apiResult/{route}");
        var text = await result.Content.ReadAsStringAsync();

        Assert.Equal(expectedStatusCode, (int)result.StatusCode);
        // Assert.Equal("application/json", result.Content.Headers.ContentType?.MediaType);

        Assert.Equal(expectedData, text);
    }

    [Fact]
    public async Task GetApiResultGenericSubclassWithoutNestedWrappingAsync()
    {
        var result = await Client.GetAsync("/apiResult/apiResultGenericSubclass");
        var text = await result.Content.ReadAsStringAsync();

        Assert.Equal(200, (int)result.StatusCode);
        Assert.Equal("application/json", result.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(0, root.GetProperty("code").GetInt32());
        Assert.Equal("自定义结果", root.GetProperty("msg").GetString());
        Assert.Equal(7, root.GetProperty("data").GetInt32());
        Assert.Equal("custom", root.GetProperty("marker").GetString());
    }

    /// <summary>
    /// 接口内抛出 <see cref="MicroserviceFrameworkFriendlyException"/> 时，
    /// 全局异常过滤器应返回 HTTP 200 + ApiResult 错误结构。
    /// </summary>
    [Fact]
    public async Task ThrowFriendlyException()
    {
        var result = await Client.GetAsync("/apiResult/friendlyException");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(200, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":2,"msg":"业务处理失败","data":null}
                     """, text);
        Assert.False(result.Headers.Contains("X-Correlation-ID"));
    }

    /// <summary>
    /// 接口内抛出普通异常（<see cref="InvalidOperationException"/>）时，
    /// 全局异常过滤器应返回 HTTP 500 + ApiResult 错误结构。
    /// </summary>
    [Fact]
    public async Task ThrowInvalidOperationException()
    {
        var result = await Client.GetAsync("/apiResult/invalidOperationException");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(500, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":500,"msg":"系统内部错误","data":null}
                     """, text);
    }

    /// <summary>
    /// 接口内抛出参数类异常（<see cref="ArgumentException"/>）时，
    /// 全局异常过滤器应返回 HTTP 500 + ApiResult 错误结构。
    /// </summary>
    [Fact]
    public async Task ThrowArgumentException()
    {
        var result = await Client.GetAsync("/apiResult/argumentException");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(500, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":500,"msg":"系统内部错误","data":null}
                     """, text);
    }

    [Fact]
    public async Task ThrowUnauthorizedExceptionReturnsForbiddenApiResult()
    {
        var result = await Client.GetAsync("/apiResult/unauthorizedException");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(403, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":403,"msg":"无权访问","data":null}
                     """, text);
    }

    [Fact]
    public async Task ThrowUnexpectedExceptionHidesInternalDetails()
    {
        var result = await Client.GetAsync("/apiResult/unexpectedException");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(500, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":500,"msg":"系统内部错误","data":null}
                     """, text);
        Assert.DoesNotContain("数据库连接字符串", text);
        Assert.DoesNotContain("InvalidOperationException", text);
    }
}
