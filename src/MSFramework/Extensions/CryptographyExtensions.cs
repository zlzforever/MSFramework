using System.Security.Cryptography;
using System.Text;
using MSFramework.Common;

namespace MSFramework.Extensions
{
	/// <summary>
	/// 字符串Hash操作类
	/// </summary>
	public static class CryptographyExtensions
	{
		/// <summary>
		/// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeMD5(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));
			
			encoding ??= Encoding.UTF8;

			var bytes = encoding.GetBytes(value);
			return bytes.ComputeMD5();
		}

		/// <summary>
		/// 获取字节数组的MD5哈希值
		/// </summary>
		public static string ComputeMD5(this byte[] bytes)
		{
			bytes.NotNull(nameof(bytes));
			var sb = new StringBuilder();
			MD5 hash = new MD5CryptoServiceProvider();
			bytes = hash.ComputeHash(bytes);
			foreach (var b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA1(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA1Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA256(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA256Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA512(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA512Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}
	}
}