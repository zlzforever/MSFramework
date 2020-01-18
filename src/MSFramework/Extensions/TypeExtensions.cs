using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MSFramework.Extensions
{
	public static class TypeExtensions
	{
		/// <summary>
		/// 通过类型转换器获取Nullable类型的基础类型
		/// </summary>
		/// <param name="type"> 要处理的类型对象 </param>
		/// <returns> </returns>
		public static Type GetUnNullableType(this Type type)
		{
			if (IsNullableType(type))
			{
				var nullableConverter = new NullableConverter(type);
				return nullableConverter.UnderlyingType;
			}

			return type;
		}

		/// <summary>
		/// 检查指定指定类型成员中是否存在指定的Attribute特性
		/// </summary>
		/// <typeparam name="T">要检查的Attribute特性类型</typeparam>
		/// <param name="memberInfo">要检查的类型成员</param>
		/// <param name="inherit">是否从继承中查找</param>
		/// <returns>是否存在</returns>
		public static bool HasAttribute<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
		{
			return memberInfo.IsDefined(typeof(T), inherit);
		}

		/// <summary>
		/// 判断类型是否为Nullable类型
		/// </summary>
		/// <param name="type"> 要处理的类型 </param>
		/// <returns> 是返回True，不是返回False </returns>
		public static bool IsNullableType(this Type type)
		{
			return ((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		public static string GetGenericTypeName(this Type type)
		{
			string typeName;

			if (type.IsGenericType)
			{
				var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
				typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
			}
			else
			{
				typeName = type.Name;
			}

			return typeName;
		}

		public static string GetGenericTypeName(this object @object)
		{
			return @object.GetType().GetGenericTypeName();
		}

		public static Type[] GetImplementedInterfaces(this Type type, params Type[] exceptInterfaces)
		{
			var interfaceTypes = type.GetInterfaces().Where(t => !exceptInterfaces.Contains(t)).ToArray();
			for (var index = 0; index < interfaceTypes.Length; index++)
			{
				var interfaceType = interfaceTypes[index];
				if (interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition &&
				    interfaceType.FullName == null)
				{
					interfaceTypes[index] = interfaceType.GetGenericTypeDefinition();
				}
			}

			return interfaceTypes;
		}
	}
}