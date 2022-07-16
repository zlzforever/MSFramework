using System.IO;
using System.Security.Cryptography;
using System.Text;
using MicroserviceFramework.Runtime;
using MicroserviceFramework.Utilities;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Security.Cryptography
{
	public static class CryptographyUtilities
	{
		/// <summary>
		/// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeMD5(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			encoding ??= Encoding.UTF8;

			var bytes = encoding.GetBytes(value);
			return ComputeMD5(bytes);
		}

		public static string ComputeMD5(Stream stream)
		{
			Check.NotNull(stream, nameof(stream));

			var hash = MD5.Create();
			var bytes = hash.ComputeHash(stream);
			return bytes.ToHex();
		}

		/// <summary>
		/// 获取字节数组的MD5哈希值
		/// </summary>
		public static string ComputeMD5(byte[] bytes)
		{
			Check.NotNull(bytes, nameof(bytes));

			var hash = MD5.Create();
			bytes = hash.ComputeHash(bytes);
			return bytes.ToHex();
		}

		/// <summary>
		/// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA1(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var hash = SHA1.Create();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			return bytes.ToHex();
		}

		/// <summary>
		/// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA256(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var hash = SHA256.Create();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			return bytes.ToHex();
		}

		/// <summary>
		/// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
		/// </summary>
		public static string ComputeSHA512(string value, Encoding encoding = null)
		{
			Check.NotNull(value, nameof(value));

			var hash = SHA512.Create();
			encoding ??= Encoding.UTF8;

			var bytes = hash.ComputeHash(encoding.GetBytes(value));
			return bytes.ToHex();
		}
	}
}