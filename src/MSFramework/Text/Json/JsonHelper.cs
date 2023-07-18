using System;
using System.IO;
using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

public class JsonHelper : IJsonHelper
{
    private readonly JsonSerializerOptions _options;

    public static IJsonHelper Create()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeOffsetJsonConverter());
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = false;

        // comments by lewis at 20230714
        // 不应该使用这个功能， 如果一个字典的键值中， 同时有 a 和 A， 会导致有两个 a 在序列化结果中
        // options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        return new JsonHelper(options);
    }

    public JsonHelper(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public byte[] SerializeToUtf8Bytes(object obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj, _options);
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public T Deserialize<T>(Stream json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public object Deserialize(string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, _options);
    }
}
