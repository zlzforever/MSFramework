using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Serialization;

public class PagedResultJsonConverter : JsonConverter<IPagedResult>
{
    private readonly Func<IPagedResult, object> _mapper;

    public PagedResultJsonConverter(Func<IPagedResult, object> mapper = null)
    {
        _mapper = mapper;
    }

    public override IPagedResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IPagedResult value, JsonSerializerOptions options)
    {
        if (_mapper == null)
        {
            var str = JsonSerializer.Serialize(
                new PagedResultWrapper(value.Page, value.Limit, value.Total, value)
                , options);
            writer.WriteRawValue(str);
        }
        else
        {
            var result = _mapper(value);
            if (result is IPagedResult)
            {
                // 会导致死循环
                throw new NotSupportedException("分页转换器不能返回 IPagedResult 数据");
            }

            var str = JsonSerializer.Serialize(result, options);
            writer.WriteRawValue(str);
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class PagedResultWrapper
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public IEnumerable Data { get; set; }

        public PagedResultWrapper(int page, int limit, int total, IEnumerable data)
        {
            Page = page;
            Limit = limit;
            Total = total;
            Data = data;
        }
    }
}
