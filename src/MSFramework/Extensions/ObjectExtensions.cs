using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MSFramework.Extensions
{
	/// <summary>
	/// 基类型<see cref="object"/>扩展辅助操作类
	/// </summary>
	public static class ObjectExtensions
	{
		#region 公共方法

		/// <summary>
		/// 把对象类型转换为指定类型
		/// </summary>
		/// <param name="value"></param>
		/// <param name="conversionType"></param>
		/// <returns></returns>
		public static object CastTo(this object value, Type conversionType)
		{
			if (value == null)
			{
				return null;
			}

			if (conversionType.IsNullableType())
			{
				conversionType = conversionType.GetUnNullableType();
			}

			if (conversionType.IsEnum)
			{
				return Enum.Parse(conversionType, value.ToString());
			}

			if (conversionType == typeof(Guid))
			{
				return Guid.Parse(value.ToString());
			}

			return Convert.ChangeType(value, conversionType);
		}

		/// <summary>
		/// 把对象类型转化为指定类型
		/// </summary>
		/// <typeparam name="T"> 动态类型 </typeparam>
		/// <param name="value"> 要转化的源对象 </param>
		/// <returns> 转化后的指定类型的对象，转化失败引发异常。 </returns>
		public static T CastTo<T>(this object value)
		{
			if (value == null && default(T) == null)
			{
				return default(T);
			}

			if (value.GetType() == typeof(T))
			{
				return (T) value;
			}

			object result = CastTo(value, typeof(T));
			return (T) result;
		}

		/// <summary>
		/// 把对象类型转化为指定类型，转化失败时返回指定的默认值
		/// </summary>
		/// <typeparam name="T"> 动态类型 </typeparam>
		/// <param name="value"> 要转化的源对象 </param>
		/// <param name="defaultValue"> 转化失败返回的指定默认值 </param>
		/// <returns> 转化后的指定类型对象，转化失败时返回指定的默认值 </returns>
		public static T CastTo<T>(this object value, T defaultValue)
		{
			try
			{
				return CastTo<T>(value);
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// 将对象[主要是匿名对象]转换为dynamic
		/// </summary>
		public static dynamic ToDynamic(this object value)
		{
			dynamic d = value;
			return d;
		}

		/// <summary>
		/// 对象深度拷贝，复制出一个数据一样，但地址不一样的新版本
		/// </summary>
		public static T DeepClone<T>(this T obj) where T : class
		{
			if (obj == null)
			{
				return default(T);
			}

			if (typeof(T).HasAttribute<SerializableAttribute>())
			{
				throw new NotSupportedException(
					"当前对象未标记特性“{0}”，无法进行DeepClone操作".FormatWith(typeof(SerializableAttribute)));
			}

			BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				formatter.Serialize(ms, obj);
				ms.Seek(0L, SeekOrigin.Begin);
				return (T) formatter.Deserialize(ms);
			}
		}

		#endregion
	}
}