using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.Text.Json;
using Xunit;

namespace MSFramework.Tests;

public class TextJsonSerializerTests
{
    [Fact]
    public void Serialize_ReturnsCorrectJsonString()
    {
        var serializer = TextJsonSerializer.Create();
        var obj = new { Name = "Test", Value = 123 };
        var result = serializer.Serialize(obj);
        Assert.Equal("{\"name\":\"Test\",\"value\":123}", result);
    }

    [Fact]
    public void SerializeToUtf8Bytes_ReturnsCorrectUtf8Bytes()
    {
        var serializer = TextJsonSerializer.Create();
        var obj = new { Name = "Test", Value = 123 };
        var result = serializer.SerializeToUtf8Bytes(obj);
        var expected = new byte[]
        {
            123, 34, 110, 97, 109, 101, 34, 58, 34, 84, 101, 115, 116, 34, 44, 34, 118, 97, 108, 117, 101, 34, 58,
            49, 50, 51, 125
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Deserialize_ReturnsCorrectObject()
    {
        var serializer = TextJsonSerializer.Create();
        var json = "{\"name\":\"Test\",\"value\":123}";
        var result = serializer.Deserialize<Dictionary<string, object>>(json);
        Assert.Equal("Test", result["name"].ToString());
        Assert.Equal(123, ((JsonElement)result["value"]).GetInt32());
    }

    [Fact]
    public void DeserializeFromStream_ReturnsCorrectObject()
    {
        var serializer = TextJsonSerializer.Create();
        var json = "{\"name\":\"Test\",\"value\":123}";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var result = serializer.Deserialize<Dictionary<string, object>>(stream);
        Assert.Equal("Test", result["name"].ToString());
        Assert.Equal(123, ((JsonElement)result["value"]).GetInt32());
    }

    [Fact]
    public void DeserializeWithType_ReturnsCorrectObject()
    {
        var serializer = TextJsonSerializer.Create();
        var json = "{\"name\":\"Test\",\"value\":123}";
        var result = serializer.Deserialize(json, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
        Assert.Equal("Test", result["name"].ToString());
        Assert.Equal(123, ((JsonElement)result["value"]).GetInt32());
    }
}
