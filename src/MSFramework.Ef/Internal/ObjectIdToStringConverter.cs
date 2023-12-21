using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

namespace MicroserviceFramework.Ef.Internal;

public class ObjectIdToStringConverter(ConverterMappingHints mappingHints = null)
    : ValueConverter<ObjectId, string>(ToString(), ToObjectId(), mappingHints ?? DefaultHints)
{
    private static new Expression<Func<ObjectId, string>> ToString()
        => v => v.ToString();

    private static Expression<Func<string, ObjectId>> ToObjectId()
        => v => v == null ? default : new ObjectId(v);

    private static readonly ConverterMappingHints DefaultHints = new(36,
        null, null, null,
        (_, _) => new ObjectIdValueGenerator());
}
