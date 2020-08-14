using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MSFramework.Domain;

namespace MSFramework.Ef.Extensions
{
	public static class PropertyBuilderExtensions
	{
		public static PropertyBuilder<TProperty> UseEnumeration<TProperty>(this PropertyBuilder<TProperty> builder)
			where TProperty : Enumeration
		{
			builder.HasConversion(new ValueConverter<TProperty, string>(
				v => v.Id,
				v => Enumeration.FromValue<TProperty>(v)));
			return builder;
		}

		public static PropertyBuilder<DateTimeOffset?> UseUnixTimeSeconds(this PropertyBuilder<DateTimeOffset?> builder)
		{
			builder.HasConversion(new ValueConverter<DateTimeOffset?, long?>(
				v => v.HasValue ? v.Value.ToUnixTimeSeconds() : default,
				v => v.HasValue ? DateTimeOffset.FromUnixTimeSeconds(v.Value) : default));
			builder.HasColumnType("int");
			return builder;
		}

		public static PropertyBuilder<DateTimeOffset> UseUnixTimeSeconds(this PropertyBuilder<DateTimeOffset> builder)
		{
			builder.HasConversion(new ValueConverter<DateTimeOffset?, long?>(
				v => v.HasValue ? v.Value.ToUnixTimeSeconds() : default,
				v => v.HasValue ? DateTimeOffset.FromUnixTimeSeconds(v.Value) : default));
			builder.HasColumnType("int");
			return builder;
		}
	}
}