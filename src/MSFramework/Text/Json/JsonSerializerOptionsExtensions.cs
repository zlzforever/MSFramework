using System.Text.Json;
using MicroserviceFramework.Text.Json.Converters;

namespace MicroserviceFramework.Text.Json;

/// <summary>
/// <see cref="JsonSerializerOptions"/> 扩展方法，注册 MSFramework 默认转换器。
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// 添加 MSFramework 默认的 JSON 转换器（ObjectId、Enumeration、DateTime、DateTimeOffset）。
    /// </summary>
    /// <param name="options">序列化选项</param>
    public static void AddDefaultConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new ObjectIdJsonConverter());
        options.Converters.Add(new EnumerationJsonConverterFactory());
        options.Converters.Add(new DateTimeJsonConverter());
        options.Converters.Add(new DateTimeOffsetJsonConverter());
    }
}
