using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// ObjectId 到 string 的 EF Core 值转换器
/// </summary>
public class ObjectIdToStringConverter()
    : ValueConverter<ObjectId, string>(ToStringValue(), ToObjectId())
{
    private static Expression<Func<ObjectId, string>> ToStringValue()
        => v => v.ToString();

    private static Expression<Func<string, ObjectId>> ToObjectId()
        => v => v == null ? default : ObjectId.Parse(v);
}
