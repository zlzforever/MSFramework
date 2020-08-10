using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MSFramework.Domain;
using MSFramework.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSFramework.AspNetCore.Infrastructure
{
	public class EnumerationConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else if (value.GetType().IsSubclassOf(typeof(Enumeration)))
			{
				writer.WriteValue(((Enumeration) value).Id);
			}
			else
			{
				throw new MSFrameworkException(122, $" no support json output");
			}
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
			JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			var vaule = token?.ToString();
			var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
			var enumType = objectType;
			if (isNullable)
			{
				enumType = objectType.GetGenericArguments().FirstOrDefault();
			}

			if (vaule.IsNullOrEmpty())
			{
				if (isNullable)
				{
					return null;
				}

				throw new MSFrameworkException(122, $" {reader.Path} 不支持空值");
			}

			try
			{
				var matchingItem = GetAll(enumType).FirstOrDefault(i => i.Id == vaule);
				if (matchingItem != null)
				{
					return matchingItem;
				}
			}
			catch (Exception e)
			{
				// 异常数据，不允许绑定
				throw new MSFrameworkException(122, $" {reader.Path} 不支持绑定值 {vaule}");
			}

			// 异常数据，不允许绑定
			throw new MSFrameworkException(122, $" {reader.Path} 不支持绑定值 {vaule}");
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsSubclassOf(typeof(Enumeration));
		}

		static IEnumerable<Enumeration> GetAll(Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration))).Select(f => (Enumeration) f.GetValue(null));
		}
	}
}