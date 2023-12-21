using System;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

public class TextJsonSerializer(JsonSerializerOptions options) : IJsonSerializer
{
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeOffsetJsonConverter());
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = false;
        return options;
    }

    public static IJsonSerializer Create()
    {
        var options = CreateDefaultOptions();

        // comments by lewis at 20230714
        // 不应该使用这个功能， 如果一个字典的键值中， 同时有 a 和 A， 会导致有两个 a 在序列化结果中
        // options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        return new TextJsonSerializer(options);
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, options);
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public T Deserialize<T>(Stream json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public object Deserialize(string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, options);
    }
}
