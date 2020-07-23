using System;
using System.Collections.Concurrent;
using System.Reflection;
using MSFramework.AspNetCore.Api;

namespace MSFramework.AspNetCore.Extensions
{
	public static class QueryExtensions
	{
		static readonly ConcurrentDictionary<Type, PropertyInfo[]> Dict =
			new ConcurrentDictionary<Type, PropertyInfo[]>();

		static readonly ConcurrentDictionary<Type, dynamic> DefaultValueDict =
			new ConcurrentDictionary<Type, dynamic>();

		public static bool IsNullOrEmpty(this IQuery query)
		{
			if (query == null)
			{
				return true;
			}

			var properties = Dict.GetOrAdd(query.GetType(), type => type.GetProperties());
			foreach (var property in properties)
			{
				var value = property.GetValue(query);
				if (value == null)
				{
					continue;
				}

				var defaultValue = DefaultValueDict.GetOrAdd(property.PropertyType, type =>
				{
					if (type == null || !type.IsValueType || type == typeof(void))
					{
						return null;
					}

					if (type.IsPrimitive || !type.IsNotPublic)
					{
						try
						{
							return Activator.CreateInstance(type);
						}
						catch (Exception e)
						{
							throw new ArgumentException(
								"{" + MethodBase.GetCurrentMethod() +
								"} Error:\n\nThe Activator.CreateInstance method could not " +
								"create a default instance of the supplied value type <" + type +
								"> (Inner Exception message: \"" + e.Message + "\")", e);
						}
					}

					throw new ArgumentException("{" + MethodBase.GetCurrentMethod() +
					                            "} Error:\n\nThe supplied value type <" + type +
					                            "> is not a publicly-visible type, so the default value cannot be retrieved");
				});

				if (!value.Equals(defaultValue))
				{
					return false;
				}
			}

			return true;
		}
	}
}