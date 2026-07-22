using System;
using System.Linq.Expressions;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal;

/// <summary>
/// Enumeration 枚举类型到 string 的 EF Core 值转换器
/// </summary>
public class EnumerationToStringConverter<T>()
    : ValueConverter<T, string>(ToStringValue(), ToEnumeration()) where T : Enumeration
{
    private static Expression<Func<T, string>> ToStringValue()
        => v => v.ToString();

    private static Expression<Func<string, T>> ToEnumeration()
        => v => string.IsNullOrEmpty(v) ? null : Enumeration.FromValue<T>(v);
}
