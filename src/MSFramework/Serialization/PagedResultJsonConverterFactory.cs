using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Serialization;

public class PagedResultJsonConverterFactory
    : JsonConverterFactory
{
    private static readonly Type PagedResultType = typeof(PagedResult<>);

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == PagedResultType;
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var genericArgument = typeToConvert.GetGenericArguments()[0];
        return Activator.CreateInstance(typeof(PagedResultJsonConverter<>).MakeGenericType(genericArgument)) as
            JsonConverter;
    }
}
