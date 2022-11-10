using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Common;

namespace MicroserviceFramework.Serialization;

internal class PagedResultJsonConverter<T> : JsonConverter<PagedResult<T>>
{
    public override PagedResult<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, PagedResult<T> value, JsonSerializerOptions options)
    {
        var result = new InternalPagedResult<T>(value.Page, value.Limit, value.Total, value.Data);
        var str = JsonSerializer.Serialize(result, options);
        writer.WriteRawValue(str);
    }

    public record InternalPagedResult<TEntity>(int Page, int Limit, int Count, IEnumerable<TEntity> Data)
    {
        public IEnumerable<TEntity> Data { get; } = Data;

        /// <summary>
        /// 总计
        /// </summary>
        public int Count { get; } = Count;

        /// <summary>
        /// 当前页数 
        /// </summary>
        public int Page { get; } = Page;

        /// <summary>
        /// 每页数据量 
        /// </summary>
        public int Limit { get; } = Limit;
    }
}
