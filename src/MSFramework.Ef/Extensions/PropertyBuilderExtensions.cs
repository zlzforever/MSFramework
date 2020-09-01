using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Extensions
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

		public static PropertyBuilder<string> UseNotNull(this PropertyBuilder<string> builder)
		{
			builder.HasConversion(new ValueConverter<string, string>(
				v => v,
				v => string.IsNullOrWhiteSpace(v) ? string.Empty : v));
			return builder;
		}
	}
}