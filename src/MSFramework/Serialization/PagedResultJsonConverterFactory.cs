using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Serialization;

public class PagedResultJsonConverterFactory
    : JsonConverterFactory
{
    private static readonly Type PagedResultType = typeof(IPagedResult);
    private readonly Func<IPagedResult, object> _mapper;

    public PagedResultJsonConverterFactory(Func<IPagedResult, object> mapper = null)
    {
        _mapper = mapper;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(PagedResultType);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new PagedResultJsonConverter(_mapper);
    }
}
