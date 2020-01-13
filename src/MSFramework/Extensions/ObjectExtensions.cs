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
			if (value == null)
			{
				return default(T);
			}

			if (value.GetType() == typeof(T))
			{
				return (T) value;
			}
			else
			{
				var result = CastTo(value, typeof(T));
				return (T) result;
			}
		}

		#endregion
	}
}