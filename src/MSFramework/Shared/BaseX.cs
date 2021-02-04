using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Shared
{
	/// <summary>
	/// 将一个数字转成随机字符串
	/// https://github.com/WinterChen/go_project/tree/master/base34
	/// </summary>
	public static class BaseX
	{
		private static string _codes;
		private static Dictionary<char, int> _cache;

		internal static string GetCodes()
		{
			return (string) _codes.Clone();
		}

		internal static string GetRandomCodes()
		{
			return new("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Shuffle().ToArray());
		}

		static BaseX()
		{
			Load();
		}

		public static void Load(string codes = null)
		{
			_codes = !string.IsNullOrWhiteSpace(codes)
				? codes
				: "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			_cache = new Dictionary<char, int>();
			for (var i = 0; i < _codes.Length; ++i)
			{
				var value = _codes[i];
				_cache[value] = i;
			}
		}

		public static string ToString(int number, int count = 6)
		{
			var quotient = number;
			var bytes = new List<char>();
			while (quotient != 0)
			{
				var mod = quotient % _codes.Length;
				quotient /= _codes.Length;
				bytes.Add(_codes[mod]);
			}

			if (bytes.Count > count)
			{
				throw new ArgumentOutOfRangeException(nameof(number));
			}
			else
			{
				var builder = new StringBuilder(count);
				for (var i = 0; i < count; i++)
				{
					builder.Append(i < count - bytes.Count ? _codes[0] : bytes[count - i - 1]);
				}

				return builder.ToString();
			}
		}

		public static int ToInt32(string str)
		{
			Check.NotEmpty(str, nameof(str));

			int res = default;
			var r = 0;
			for (var i = str.Length - 1; i >= 0; i--)
			{
				var c = str[i];
				if (!_cache.ContainsKey(c))
				{
					throw new MicroserviceFrameworkException($"字符 {c} 不合法");
				}

				var v = _cache[c];

				var b = 1;
				for (var j = 0; j < r; j++)
				{
					b *= _codes.Length;
				}

				res += b * v;
				r++;
			}

			return res;
		}

		/// <summary>
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		/// <exception cref="MicroserviceFrameworkException"></exception>
		public static long ToInt64(string str)
		{
			Check.NotEmpty(str, nameof(str));

			long res = default;
			long r = 0;
			for (var i = str.Length - 1; i >= 0; i--)
			{
				var c = str[i];
				if (!_cache.ContainsKey(c))
				{
					throw new MicroserviceFrameworkException($"字符 {c} 不合法");
				}

				var v = _cache[c];

				long b = 1;
				for (long j = 0; j < r; j++)
				{
					b *= _codes.Length;
				}

				res += b * v;
				r++;
			}

			return res;
		}
	}
}