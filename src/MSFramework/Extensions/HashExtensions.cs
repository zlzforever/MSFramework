using System.Security.Cryptography;
using System.Text;
using MSFramework.Data;

namespace MSFramework.Extensions
{
	/// <summary>
	/// 字符串Hash操作类
	/// </summary>
	public static class HashExtensions
	{
		/// <summary>
		/// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToMd5(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			byte[] bytes = encoding.GetBytes(value);
			return bytes.ToMd5();
		}

		/// <summary>
		/// 获取字节数组的MD5哈希值
		/// </summary>
		public static string ToMd5(this byte[] bytes)
		{
			bytes.NotNull(nameof(bytes));
			StringBuilder sb = new StringBuilder();
			MD5 hash = new MD5CryptoServiceProvider();
			bytes = hash.ComputeHash(bytes);
			foreach (byte b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha1(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA1Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (byte b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha256(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA256Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (byte b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha512(this string value, Encoding encoding = null)
		{
			value.NotNull(nameof(value));

			var sb = new StringBuilder();
			var hash = new SHA512Managed();
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

			byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (byte b in bytes)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString();
		}
	}
}