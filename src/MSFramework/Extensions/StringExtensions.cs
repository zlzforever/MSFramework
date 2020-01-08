using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MSFramework.Data;
using Newtonsoft.Json;

namespace MSFramework.Extensions
{
	public static class StringExtensions
	{
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string FormatWith(this string format, params object[] args)
		{
			format.NotNull("format");
			return string.Format(CultureInfo.CurrentCulture, format, args);
		}

		/// <summary>
		/// 判断指定路径是否图片文件
		/// </summary>
		public static bool IsImageFile(this string filename)
		{
			if (!File.Exists(filename))
			{
				return false;
			}

			byte[] filedata = File.ReadAllBytes(filename);
			if (filedata.Length == 0)
			{
				return false;
			}

			ushort code = BitConverter.ToUInt16(filedata, 0);
			switch (code)
			{
				case 0x4D42: //bmp
				case 0xD8FF: //jpg
				case 0x4947: //gif
				case 0x5089: //png
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// 以指定字符串作为分隔符将指定字符串分隔成数组
		/// </summary>
		/// <param name="value">要分割的字符串</param>
		/// <param name="separator">字符串类型的分隔符</param>
		/// <param name="removeEmptyEntries">是否移除数据中元素为空字符串的项</param>
		/// <returns>分割后的数据</returns>
		public static string[] Split(this string value, string separator, bool removeEmptyEntries = false)
		{
			return value.Split(new[] {separator},
				removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
		}

		public static T ToObject<T>(this string json)
		{
			json.NotNull("json");
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static object ToObject(this string json, Type type)
		{
			json.NotNull("json");
			return JsonConvert.DeserializeObject(json, type);
		}

		public static byte[] ToBytes(this string value, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			return encoding.GetBytes(value);
		}

		public static string ConvertToString(this byte[] bytes, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			return encoding.GetString(bytes);
		}

		public static string ToBase64String(this string source, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			return Convert.ToBase64String(encoding.GetBytes(source));
		}

		/// <summary>
		/// 将字符串进行UrlDecode解码
		/// </summary>
		/// <param name="source">待UrlDecode解码的字符串</param>
		/// <returns>UrlDecode解码后的字符串</returns>
		public static string UrlDecode(this string source)
		{
			return HttpUtility.UrlDecode(source);
		}

		public static string UrlEncode(this string source)
		{
			return HttpUtility.UrlEncode(source);
		}

		public static string HtmlDecode(this string source)
		{
			return HttpUtility.HtmlDecode(source);
		}

		/// <summary>
		/// 将字符串进行HtmlEncode编码
		/// </summary>
		/// <param name="source">待HtmlEncode编码的字符串</param>
		/// <returns>HtmlEncode编码后的字符串</returns>
		public static string HtmlEncode(this string source)
		{
			return HttpUtility.HtmlEncode(source);
		}

		public static string ToHexString(this string source, Encoding encoding = null)
		{
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			byte[] bytes = encoding.GetBytes(source);
			return bytes.ToHexString();
		}

		public static string ToHexString(this byte[] bytes)
		{
			return bytes.Aggregate(string.Empty, (current, t) => current + t.ToString("X2"));
		}

		public static byte[] ToHexBytes(this string hexString)
		{
			hexString = hexString.Replace(" ", "");
			if (hexString.Length % 2 != 0)
			{
				hexString = hexString ?? "";
			}

			byte[] bytes = new byte[hexString.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}

			return bytes;
		}

		/// <summary>
		/// 将字符串进行Unicode编码，变成形如“\u7f16\u7801”的形式
		/// </summary>
		/// <param name="source">要进行编号的字符串</param>
		public static string ToUnicode(this string source)
		{
			Regex regex = new Regex(@"[^\u0000-\u00ff]");
			return regex.Replace(source, m => string.Format(@"\u{0:x4}", (short) m.Value[0]));
		}
	}
}