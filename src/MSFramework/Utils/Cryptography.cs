using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Utils;

/// <summary>
/// 加密解密工具类，提供 MD5/SHA1/SHA256/SHA512 哈希和 AES 加解密
/// </summary>
public static class Cryptography
{
    /// <summary>
    /// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeMD5(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        encoding ??= Encoding.UTF8;
        return ComputeMD5(encoding.GetBytes(value));
    }

    /// <summary>
    /// 计算字节数组的 MD5 哈希值，返回大写十六进制字符串
    /// </summary>
    /// <param name="data">待计算的字节数组</param>
    /// <returns>MD5 哈希值（大写十六进制）</returns>
    public static string ComputeMD5(byte[] data)
    {
        var bytes = MD5.HashData(data);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 异步计算流的 MD5 哈希值，返回大写十六进制字符串
    /// </summary>
    /// <param name="stream">待计算的流</param>
    /// <returns>MD5 哈希值（大写十六进制）</returns>
    public static async Task<string> ComputeMD5Async(Stream stream)
    {
        var bytes = await MD5.HashDataAsync(stream);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA1(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        encoding ??= Encoding.UTF8;
        var bytes = SHA1.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA256(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        encoding ??= Encoding.UTF8;
        var bytes = SHA256.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    public static string ComputeSHA512(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        encoding ??= Encoding.UTF8;

        var bytes = SHA512.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// AES 算法解密(ECB模式) 将密文base64解码进行解密，返回明文
    /// </summary>
    /// <param name="text">密文</param>
    /// <param name="key">密钥</param>
    /// <returns>明文</returns>
    public static string AesDecryptToString(string text, string key)
    {
        return Encoding.UTF8.GetString(AesDecrypt(text, key));
    }

    /// <summary>
    /// AES 算法解密(ECB模式) 将密文base64解码进行解密，返回明文
    /// </summary>
    /// <param name="text">密文</param>
    /// <param name="key">密钥</param>
    /// <returns>明文</returns>
    public static byte[] AesDecrypt(string text, string key)
    {
        using var aes = CreateAes(key);
        return AesDecrypt(aes, text);
    }

    /// <summary>
    /// 根据密钥创建 AES 加密算法实例
    /// </summary>
    /// <param name="key">密钥字符串（UTF-8 编码）</param>
    /// <param name="cipherMode">加密模式，默认 ECB</param>
    /// <param name="paddingMode">填充模式，默认 PKCS7</param>
    /// <returns>AES 加密器实例</returns>
    public static Aes CreateAes(string key, CipherMode cipherMode = CipherMode.ECB,
        PaddingMode paddingMode = PaddingMode.PKCS7)
    {
        var keyArray = Encoding.UTF8.GetBytes(key);
        var aes = Aes.Create();
        aes.Key = keyArray;
        aes.Mode = cipherMode;
        aes.Padding = paddingMode;
        return aes;
    }

    /// <summary>
    /// 使用指定的 AES 实例解密密文
    /// </summary>
    /// <param name="aes">AES 加密器实例</param>
    /// <param name="text">Base64 编码的密文</param>
    /// <returns>解密后的字节数组</returns>
    public static byte[] AesDecrypt(Aes aes, string text)
    {
        var toEncryptArray = Convert.FromBase64String(text);
        using var decrypt = aes.CreateDecryptor();
        return decrypt.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
}
