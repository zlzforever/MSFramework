using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// 下划线命名法
		/// todo: 优化
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToUnderScoreCase(this string value)
		{
			var array = value.ToList();
			if (char.IsUpper(array[0]))
			{
				array[0] = char.ToLower(array[0]);
			}

			while (true)
			{
				var index = GetIndexOfFirstUpper(array);
				if (index <= 0)
				{
					break;
				}

				var c = array[index];
				array[index] = char.ToLower(c);
				if (array.ElementAtOrDefault(index - 1) != '_')
				{
					array.Insert(index, '_');
				}
			}

			return new string(array.ToArray());
		}

		private static int GetIndexOfFirstUpper(List<char> array)
		{
			var index = -1;
			for (var i = 0; i < array.Count; ++i)
			{
				if (!char.IsUpper(array[i]))
				{
					continue;
				}

				index = i;
				break;
			}

			return index;
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}
	}
}