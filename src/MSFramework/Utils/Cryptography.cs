using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Utils;

public static class Cryptography
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

    public static async Task<string> ComputeMD5Async(Stream stream)
    {
        Check.NotNull(stream, nameof(stream));

        stream.Seek(0, SeekOrigin.Begin);
        var data = new byte[stream.Length];
        _ = await stream.ReadAsync(data, 0, data.Length);

        var bytes = MD5.HashData(data);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字节数组的MD5哈希值
    /// </summary>
    public static string ComputeMD5(byte[] bytes)
    {
        Check.NotNull(bytes, nameof(bytes));

        var hashBytes = MD5.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA1(string value, Encoding encoding = null)
    {
        Check.NotNull(value, nameof(value));

        encoding ??= Encoding.UTF8;
        var bytes = SHA1.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA256(string value, Encoding encoding = null)
    {
        Check.NotNull(value, nameof(value));

        encoding ??= Encoding.UTF8;
        var bytes = SHA256.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA512(string value, Encoding encoding = null)
    {
        Check.NotNull(value, nameof(value));

        encoding ??= Encoding.UTF8;

        var bytes = SHA512.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
