using System.Net.Http;
using System.Threading.Tasks;
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
        Assert.Equal(200, (int)result.StatusCode);
        Assert.Equal("""
                     {"errors":[{"name":"id","messages":["The id field is required."]}],"success":false,"code":1,"msg":"数据校验不通过","data":null}
                     """, text);
    }


    [Fact]
    public async Task Return452()
    {
        var result = await Client.GetAsync("/apiResult/452");
        var text = await result.Content.ReadAsStringAsync();
        Assert.Equal(452, (int)result.StatusCode);
        Assert.Equal("""
                     {"success":false,"code":-1,"msg":"","data":null}
                     """, text);
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
    public async Task GetApiResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/apiResult");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":1}
                     """, result1);
    }

    [Fact]
    public async Task GetNewtonsoftJsonApiResult()
    {
        var result1 = await Client.GetStringAsync("/apiResult/newtonsoftJson");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":1}
                     """, result1);
    }

    [Fact]
    public async Task GetApiResultGeneric()
    {
        var result1 = await Client.GetStringAsync("/apiResult/apiResultGeneric");
        Assert.Equal("""
                     {"success":true,"code":0,"msg":"","data":1}
                     """, result1);
    }
}
