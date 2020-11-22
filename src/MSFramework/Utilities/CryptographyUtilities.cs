using System.Security.Cryptography;
using System.Text;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Utilities
{
	public class CryptographyUtilities
	{
		/// <summary>
		/// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToMd5(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			encoding ??= Encoding.UTF8;

			var bytes = encoding.GetBytes(value);
			return ToMd5(bytes);
		}

		/// <summary>
		/// 获取字节数组的MD5哈希值
		/// </summary>
		public static string ToMd5(byte[] bytes)
		{
			Check.NotNull(bytes, nameof(bytes));
			var builder = new StringBuilder();
			var hash = new MD5CryptoServiceProvider();
			bytes = hash.ComputeHash(bytes);
			foreach (var b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}

		/// <summary>
		/// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha1(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var builder = new StringBuilder();
			var hash = new SHA1Managed();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha256(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var builder = new StringBuilder();
			var hash = new SHA256Managed();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}

		/// <summary>
		/// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ToSha512(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var builder = new StringBuilder();
			var hash = new SHA512Managed();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			foreach (var b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}
	}
}