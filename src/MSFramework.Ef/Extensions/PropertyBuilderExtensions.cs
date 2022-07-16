using System;
using DeepCopy;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Extensions
{
	public static class PropertyBuilderExtensions
	{
		private static readonly bool UseJsonDeepCopier;

		static PropertyBuilderExtensions()
		{
			if (Environment.GetEnvironmentVariable("MSF_EF_USE_JSON_DEEP_COPIER")?.ToLower() == "true")
			{
				UseJsonDeepCopier = true;
			}
		}

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
			JsonDataType databaseType = JsonDataType.JSON, IJsonHelper jsonHelper = null) where TProperty : class
		{
			if (jsonHelper == null)
			{
				if (Default.JsonHelper == null)
				{
					throw new ArgumentException("serializer 为空， 并且默认的序列化器也为空。");
				}

				jsonHelper = Default.JsonHelper;
			}

			var comparer = CreateValueComparer<TProperty>(jsonHelper);
			var propertyBuilder = builder
				.UsePropertyAccessMode(PropertyAccessMode.PreferField)
				.HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
			propertyBuilder.HasConversion(x => jsonHelper.Serialize(x),
				x => jsonHelper.Deserialize<TProperty>(x));
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
		/// <param name="jsonHelper"></param>
		/// <typeparam name="TProperty"></typeparam>
		/// <returns></returns>
		public static PropertyBuilder<TProperty> UseJson<TProperty>(
			this PropertyBuilder<TProperty> builder,
			Type fieldType,
			JsonDataType databaseType = JsonDataType.JSON, IJsonHelper jsonHelper = null)
			where TProperty : class
		{
			if (jsonHelper == null)
			{
				if (Default.JsonHelper == null)
				{
					throw new ArgumentException("serializer 为空， 并且默认的序列化器也为空。");
				}

				jsonHelper = Default.JsonHelper;
			}

			var comparer = CreateValueComparer<TProperty>(jsonHelper, fieldType);
			var propertyBuilder = builder
				.UsePropertyAccessMode(PropertyAccessMode.PreferField)
				.HasColumnType(databaseType == JsonDataType.JSON ? "JSON" : "JSONB");
			propertyBuilder.HasConversion(x => jsonHelper.Serialize(x),
				x => jsonHelper.Deserialize(x, fieldType) as TProperty);
			propertyBuilder.Metadata.SetValueComparer(comparer);

			return builder;
		}

		public static PropertyBuilder<TProperty> UseJson<TProperty>(
			this PropertyBuilder<TProperty> builder,
			Type fieldType,
			IJsonHelper jsonHelper, JsonDataType databaseType = JsonDataType.JSON)
			where TProperty : class
		{
			builder.UseJson(fieldType, databaseType, jsonHelper);
			return builder;
		}

		private static ValueComparer<TProperty> CreateValueComparer<TProperty>(IJsonHelper jsonHelper,
			Type targetType = null)
			where TProperty : class
		{
			return new ValueComparer<TProperty>
			(
				(l, r) => jsonHelper.Serialize(l) ==
				          jsonHelper.Serialize(r),
				v => v == null ? 0 : jsonHelper.Serialize(v).GetHashCode(),
				v => Copy(jsonHelper, v, targetType)
			);
		}

		private static TProperty Copy<TProperty>(IJsonHelper jsonHelper, TProperty v, Type targetType = null)
			where TProperty : class
		{
			if (v == null)
			{
				return default;
			}

			if (!UseJsonDeepCopier)
			{
				return DeepCopier.Copy(v);
			}

			if (targetType == null)
			{
				return jsonHelper.Deserialize<TProperty>(jsonHelper.Serialize(v));
			}

			return jsonHelper.Deserialize(
				jsonHelper.Serialize(v), targetType) as TProperty;
		}
	}
}