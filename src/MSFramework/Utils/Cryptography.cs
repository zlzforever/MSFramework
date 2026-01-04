using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace MicroserviceFramework.Utils;

/// <summary>
///
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
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ComputeMD5(byte[] data)
    {
        var bytes = MD5.HashData(data);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
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
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cipherMode"></param>
    /// <param name="paddingMode"></param>
    /// <returns></returns>
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
    ///
    /// </summary>
    /// <param name="aes"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static byte[] AesDecrypt(Aes aes, string text)
    {
        var toEncryptArray = Convert.FromBase64String(text);
        using var decrypt = aes.CreateDecryptor();
        return decrypt.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
}
