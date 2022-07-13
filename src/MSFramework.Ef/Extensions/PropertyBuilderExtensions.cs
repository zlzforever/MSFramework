using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

		public static PropertyBuilder<DateTimeOffset?> UseUnixTime(this PropertyBuilder<DateTimeOffset?> builder,
			bool milliseconds = false)
		{
			builder.HasConversion(new ValueConverter<DateTimeOffset?, long?>(
				v => v.HasValue
					? milliseconds ? v.Value.ToUnixTimeMilliseconds() : v.Value.ToUnixTimeSeconds()
					: default,
				v => v.HasValue
					? milliseconds
						? DateTimeOffset.FromUnixTimeMilliseconds(v.Value).ToLocalTime()
						: DateTimeOffset.FromUnixTimeSeconds(v.Value).ToLocalTime()
					: default));
			return builder;
		}

		public static PropertyBuilder<DateTimeOffset> UseUnixTime(this PropertyBuilder<DateTimeOffset> builder,
			bool milliseconds = false)
		{
			builder.HasConversion(new ValueConverter<DateTimeOffset, long>(
				v => milliseconds ? v.ToUnixTimeMilliseconds() : v.ToUnixTimeSeconds(),
				v => milliseconds
					? DateTimeOffset.FromUnixTimeMilliseconds(v)
					: DateTimeOffset.FromUnixTimeSeconds(v).ToLocalTime()));
			builder.IsRequired();
			builder.HasDefaultValue(DateTimeOffset.UnixEpoch);
			return builder;
		}


		public static PropertyBuilder<TProperty> UseJson<TProperty>(
			this PropertyBuilder<TProperty> builder,
			JsonDataType databaseType = JsonDataType.JSON)
		{
			var comparer = CreateValueComparer<TProperty>();
			var propertyBuilder = builder
				.UsePropertyAccessMode(PropertyAccessMode.PreferField)
				.HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
			propertyBuilder.HasConversion(x => Serialization.Default.Serializer.Serialize(x),
				x => Serialization.Default.Serializer.Deserialize<TProperty>(x));
			propertyBuilder.Metadata.SetValueComparer(comparer);
			return propertyBuilder;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="fieldType">JSON field 的数据类型。比如属性是 IReadonlyList, 其 field 可能定义为 HashSet
		/// 因此反序列化成 targetType 并使用 as 转换，肯定能转换</param>
		/// <param name="databaseType"></param>
		/// <typeparam name="TProperty"></typeparam>
		/// <returns></returns>
		public static PropertyBuilder<TProperty> UseJson<TProperty>(
			this PropertyBuilder<TProperty> builder,
			Type fieldType,
			JsonDataType databaseType = JsonDataType.JSON)
			where TProperty : class
		{
			var comparer = CreateValueComparer<TProperty>();
			var propertyBuilder = builder
				.UsePropertyAccessMode(PropertyAccessMode.PreferField)
				.HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
			propertyBuilder.HasConversion(x => Serialization.Default.Serializer.Serialize(x),
				x => Serialization.Default.Serializer.Deserialize(x, fieldType) as TProperty);
			propertyBuilder.Metadata.SetValueComparer(comparer);
			return propertyBuilder;
		}

		private static ValueComparer<TProperty> CreateValueComparer<TProperty>()
		{
			return new ValueComparer<TProperty>
			(
				(l, r) => Serialization.Default.Serializer.Serialize(l) ==
				          Serialization.Default.Serializer.Serialize(r),
				v => v == null ? 0 : Serialization.Default.Serializer.Serialize(v).GetHashCode(),
				v => Serialization.Default.Serializer.Deserialize<TProperty>(
					Serialization.Default.Serializer.Serialize(v))
			);
		}
	}
}