using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
///
/// </summary>
public class ObjectIdToStringConverter()
    : ValueConverter<ObjectId, string>(ToStringValue(), ToObjectId())
{
    private static Expression<Func<ObjectId, string>> ToStringValue()
        => v => v.ToString();

    private static Expression<Func<string, ObjectId>> ToObjectId()
        => v => v == null ? default : ObjectId.Parse(v);
}
