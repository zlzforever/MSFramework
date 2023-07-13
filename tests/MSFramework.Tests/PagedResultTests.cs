using System.Text.Json;
using MicroserviceFramework.Common;
using MicroserviceFramework.Text.Json;
using Xunit;

namespace MSFramework.Tests;

public class PagedResultTests
{
    [Fact]
    public void PagedResultSerialize1()
    {
//         var result = new PagedResult(1, 10, 100, null);
//         var json = JsonSerializer.Serialize(result);
//
//         Assert.Equal("""
// {"Data":null,"Total":100,"Page":1,"Limit":10}
// """, json);
//
//         var jsonHelper = JsonHelper.Create();
//         var json2 = jsonHelper.Serialize(result);
//         Assert.Equal("""
// {"data":null,"total":100,"page":1,"limit":10}
// """, json2);
    }


    [Fact]
    public void PagedResultSerialize2()
    {
//         var result = new PagedResult(1, 10, 100, null);
//
//         using var stream = new MemoryStream();
//         using var writer = new Utf8JsonWriter(stream);
//
//         writer.WritePagedResult(result);
//         writer.Flush();
//         var json = Encoding.UTF8.GetString(stream.ToArray());
//         Assert.Equal("""
// {"Data":null,"Total":100,"Page":1,"Limit":10}
// """, json);
    }
}
