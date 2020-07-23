using System;
using System.Collections.Generic;
using System.Text;

namespace MSFramework.Common
{
	/// <summary>
	/// 将一个数字转成随机字符串
	/// </summary>
	public static class NumberEncoding
	{
		public static string Codes = "0A1B2C3D4E5F6G7H8J9KXRYZPQSTUVWLMN";

		public static string Encode(int number, int count = 6)
		{
			var quotient = number;
			var bytes = new List<char>();
			while (quotient != 0)
			{
				var mod = quotient % Codes.Length;
				quotient /= Codes.Length;
				bytes.Add(Codes[mod]);
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
					builder.Append(i < count - bytes.Count ? Codes[0] : bytes[count - i - 1]);
				}

				return builder.ToString();
			}
		}
	}
}