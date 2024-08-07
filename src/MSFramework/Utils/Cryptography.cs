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
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        encoding ??= Encoding.UTF8;
        return ComputeMD5(encoding.GetBytes(value));
    }

    public static string ComputeMD5(byte[] data)
    {
        var bytes = MD5.HashData(data);
        return Convert.ToHexString(bytes);
    }

    public static async Task<string> ComputeMD5Async(Stream stream)
    {
        var bytes = await MD5.HashDataAsync(stream);
        return Convert.ToHexString(bytes);
    }

    // /// <summary>
    // /// 获取字节数组的MD5哈希值
    // /// </summary>
    // public static async Task<string> ComputeMD5Async(byte[] bytes)
    // {
    //     var bytes2 = await MD5.HashDataAsync(new MemoryStream(bytes));
    //     return Convert.ToHexString(bytes2);
    // }

    /// <summary>
    /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA1(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        encoding ??= Encoding.UTF8;
        var bytes = SHA1.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA256(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        encoding ??= Encoding.UTF8;
        var bytes = SHA256.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA512(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        encoding ??= Encoding.UTF8;

        var bytes = SHA512.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
