#nullable enable
using System;
using System.Linq;
using MicroserviceFramework.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroserviceFramework.Serialization.Newtonsoft.Converters;

public class EnumerationConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else if (value.GetType().IsSubclassOf(typeof(Enumeration)))
        {
            writer.WriteValue(((Enumeration)value).Id);
        }
        else
        {
            throw new MicroserviceFrameworkException(122, " no support json output");
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var value = token.ToString();
        // var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
        // var enumType = objectType;
        // if (isNullable)
        // {
        //     enumType = objectType.GetGenericArguments().FirstOrDefault();
        // }

        if (token.Type == JTokenType.None
            || token.Type == JTokenType.Null
            || token.Type == JTokenType.Undefined
            || string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            var enumeration = Enumeration.GetAll(objectType).FirstOrDefault(i => i.Id == value);
            if (enumeration != null)
            {
                return enumeration;
            }
        }
        catch (Exception)
        {
            // 异常数据，不允许绑定
            throw new MicroserviceFrameworkException(122, $"{reader.Path} 不支持绑定值 {value}");
        }

        // 异常数据，不允许绑定
        throw new MicroserviceFrameworkException(122, $"{reader.Path} 不支持绑定值 {value}");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsSubclassOf(typeof(Enumeration));
    }
}
