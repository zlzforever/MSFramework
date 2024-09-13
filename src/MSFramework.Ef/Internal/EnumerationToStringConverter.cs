using System;
using System.Linq.Expressions;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
///
/// </summary>
public class EnumerationToStringConverter<T>()
    : ValueConverter<T, string>(ToStringValue(), ToEnumeration()) where T : Enumeration
{
    private static Expression<Func<T, string>> ToStringValue()
        => v => v.ToString();

    private static Expression<Func<string, T>> ToEnumeration()
        => v => string.IsNullOrEmpty(v) ? default : Enumeration.FromValue<T>(v);
}
