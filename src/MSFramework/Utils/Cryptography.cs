using System;
using System.IO;
using System.Linq;
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
    /// <param name="value">待哈希的字符串</param>
    /// <param name="encoding">文本编码，null 时使用 <see cref="Encoding.UTF8"/></param>
    /// <returns>MD5 哈希值（大写十六进制）</returns>
    /// <remarks><b>仅适用于校验和、文件去重等非安全场景</b>（MD5 已可碰撞伪造）；
    /// 禁止用于密码存储、签名等安全用途，安全场景应使用 <see cref="ComputeSHA256"/> 及密钥派生方案</remarks>
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
    /// <remarks><b>仅适用于校验和、文件去重等非安全场景</b>，禁止用于密码存储、签名等安全用途</remarks>
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
    /// <remarks><b>仅适用于校验和、文件去重等非安全场景</b>，禁止用于密码存储、签名等安全用途</remarks>
    public static async Task<string> ComputeMD5Async(Stream stream)
    {
        var bytes = await MD5.HashDataAsync(stream);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>
    /// </summary>
    /// <param name="value">待哈希的字符串</param>
    /// <param name="encoding">文本编码，null 时使用 <see cref="Encoding.UTF8"/></param>
    /// <returns>SHA1 哈希值（大写十六进制）</returns>
    /// <remarks><b>仅适用于校验和等非安全场景</b>（SHA1 已可碰撞伪造）；
    /// 禁止用于密码存储、签名等安全用途，安全场景应使用 <see cref="ComputeSHA256"/></remarks>
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
    /// <param name="value">待哈希的字符串</param>
    /// <param name="encoding">文本编码，null 时使用 <see cref="Encoding.UTF8"/></param>
    /// <returns>SHA256 哈希值（大写十六进制）</returns>
    /// <remarks>密码存储不应直接使用哈希，应使用带盐与迭代次数的密钥派生方案（如 PBKDF2）</remarks>
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
    /// <param name="value">待哈希的字符串</param>
    /// <param name="encoding">文本编码，null 时使用 <see cref="Encoding.UTF8"/></param>
    /// <returns>SHA512 哈希值（大写十六进制）</returns>
    /// <remarks>密码存储不应直接使用哈希，应使用带盐与迭代次数的密钥派生方案（如 PBKDF2）</remarks>
    public static string ComputeSHA512(string value, Encoding encoding = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        encoding ??= Encoding.UTF8;

        var bytes = SHA512.HashData(encoding.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// AES 算法解密（历史约定的 ECB 模式）将密文 base64 解码进行解密，返回明文。
    /// 为兼容存量 ECB 密文，本方法显式使用 ECB；新加密场景不应再采用 ECB，见 <see cref="CreateAes(string, byte[], System.Security.Cryptography.CipherMode, System.Security.Cryptography.PaddingMode)"/>
    /// </summary>
    /// <param name="text">Base64 编码的密文</param>
    /// <param name="key">密钥</param>
    /// <returns>明文</returns>
    public static string AesDecryptToString(string text, string key)
    {
        return Encoding.UTF8.GetString(AesDecrypt(text, key));
    }

    /// <summary>
    /// AES 算法解密（历史约定的 ECB 模式）将密文 base64 解码进行解密，返回明文字节数组。
    /// 为兼容存量 ECB 密文，本方法显式使用 ECB；新加密场景不应再采用 ECB，见 <see cref="CreateAes(string, byte[], System.Security.Cryptography.CipherMode, System.Security.Cryptography.PaddingMode)"/>
    /// </summary>
    /// <param name="text">Base64 编码的密文</param>
    /// <param name="key">密钥</param>
    /// <returns>明文字节数组</returns>
    public static byte[] AesDecrypt(string text, string key)
    {
        // 显式 ECB：兼容历史密文，不随 CreateAes 默认值变化
        using var aes = CreateAes(key, iv: null, CipherMode.ECB);
        return AesDecrypt(aes, text);
    }

    /// <summary>
    /// 根据密钥创建 AES 算法实例（历史重载，默认 ECB）。
    /// CBC/CFB/CTS 等需要初始化向量的模式必须使用
    /// <see cref="CreateAes(string, byte[], System.Security.Cryptography.CipherMode, System.Security.Cryptography.PaddingMode)"/>
    /// 显式传入 IV；本重载仅用于兼容历史 ECB 密文解密或无需 IV 的场景。
    /// </summary>
    /// <param name="key">密钥字符串（UTF-8 编码）</param>
    /// <param name="cipherMode">加密模式，默认 ECB（仅用于兼容历史调用）；
    /// 非 ECB 模式在未提供 IV 时会抛出 <see cref="ArgumentException"/></param>
    /// <param name="paddingMode">填充模式，默认 PKCS7</param>
    /// <returns>AES 加密器实例</returns>
    /// <exception cref="ArgumentNullException">key 为 null 或空字符串时抛出</exception>
    /// <exception cref="ArgumentException">key 的 UTF-8 字节长度不是 16/24/32（AES-128/192/256）、
    /// 选择了需要 IV 的模式但未提供 IV 时抛出</exception>
    /// <remarks>
    /// 保留该重载是为了源码/行为兼容：旧代码调用 <c>CreateAes(key)</c> 仍按 ECB 工作。
    /// 新代码请使用带 IV 的四参数重载，避免 CBC/CFB/CTS 因使用自动生成的随机 IV
    /// 导致解密端解出垃圾数据。
    /// </remarks>
    [Obsolete("历史兼容重载默认 ECB；CBC/CFB/CTS 等需要 IV 的模式请使用 CreateAes(key, iv, ...)。")]
    public static Aes CreateAes(string key, CipherMode cipherMode = CipherMode.ECB,
        PaddingMode paddingMode = PaddingMode.PKCS7)
    {
        return CreateAes(key, iv: null, cipherMode, paddingMode);
    }

    /// <summary>
    /// 根据密钥与 IV 创建 AES 算法实例（推荐重载：加解密双方使用相同 IV 才能正确解密）
    /// </summary>
    /// <param name="key">密钥字符串（UTF-8 编码）</param>
    /// <param name="iv">初始化向量：ECB 模式必须传 null，其余模式必须为 16 字节（AES 块大小）</param>
    /// <param name="cipherMode">加密模式，默认 CBC</param>
    /// <param name="paddingMode">填充模式，默认 PKCS7</param>
    /// <returns>AES 加密器实例</returns>
    /// <exception cref="ArgumentNullException">key 为 null 或空字符串时抛出</exception>
    /// <exception cref="ArgumentException">key 的 UTF-8 字节长度不是 16/24/32、IV 长度不是 16、
    /// ECB 模式传入了 IV，或非 ECB 模式未提供 IV 时抛出</exception>
    public static Aes CreateAes(string key, byte[] iv, CipherMode cipherMode = CipherMode.CBC,
        PaddingMode paddingMode = PaddingMode.PKCS7)
    {
        Check.NotNullOrEmpty(key, nameof(key));

        var keyArray = Encoding.UTF8.GetBytes(key);
        if (keyArray.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException(
                $"AES 密钥须为 16/24/32 字节（UTF-8，对应 AES-128/192/256），实际为 {keyArray.Length} 字节",
                nameof(key));
        }

        if (cipherMode == CipherMode.ECB)
        {
            if (iv != null)
            {
                throw new ArgumentException("ECB 模式不接受初始化向量，请传入 null", nameof(iv));
            }
        }
        else
        {
            if (iv == null)
            {
                throw new ArgumentException(
                    $"{cipherMode} 模式必须显式提供初始化向量：依赖自动生成的随机 IV 时，" +
                    "解密端独立构造实例会因 IV 不一致解出垃圾数据（PKCS7 填充碰巧合法时无任何报错）",
                    nameof(iv));
            }

            if (iv.Length != 16)
            {
                throw new ArgumentException(
                    $"AES 初始化向量须为 16 字节（AES 块大小），实际为 {iv.Length} 字节", nameof(iv));
            }

            if (iv.All(b => b == 0))
            {
                // 全零 IV 在创建前直接校验（避免先分配实例、后因校验失败而泄漏——
                // Aes 持有非托管加密上下文，异常路径拿不到引用无法由调用方释放）
                throw new ArgumentException(
                    "CBC 模式检测到全零初始化向量：全零 IV 是\"忘设 IV\"的典型占位错误，" +
                    "创建/加密/解密三端统一拒绝", nameof(iv));
            }
        }

        // 创建后仅做赋值；如 provider 抛出异常（理论分支），释放实例后重新抛出
        Aes aes = null;
        try
        {
            aes = Aes.Create();
            aes.Key = keyArray;
            if (iv != null)
            {
                aes.IV = iv;
            }

            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            return aes;
        }
        catch
        {
            aes?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// CBC 模式全零 IV 哨兵（加密/解密使用；构造路径已在创建前校验 IV 数组）：
    /// 全零 IV 是"忘设 IV"的典型占位错误；两端一致拒绝，
    /// 避免旧策略"加密接受、解密拒绝"导致加密成功的数据无法经配套接口还原的不对称
    /// </summary>
    /// <param name="aes">待校验的 AES 实例</param>
    /// <exception cref="ArgumentException">CBC 模式且 IV 为全零时抛出</exception>
    private static void ThrowIfAllZeroIv(Aes aes)
    {
        if (aes.Mode == CipherMode.CBC && aes.IV.All(b => b == 0))
        {
            throw new ArgumentException(
                "CBC 模式检测到全零初始化向量：全零 IV 是\"忘设 IV\"的典型占位错误，" +
                "创建/加密/解密三端统一拒绝", nameof(aes));
        }
    }

    /// <summary>
    /// 使用指定的 AES 实例解密密文
    /// </summary>
    /// <param name="aes">AES 加密器实例；CBC 模式下其 IV 必须与加密端一致</param>
    /// <param name="text">Base64 编码的密文</param>
    /// <returns>解密后的字节数组</returns>
    /// <exception cref="ArgumentNullException">aes 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">CBC 模式下 IV 为全零占位值时抛出（三端统一拒绝；
    /// 典型错误：解密端未设置 IV 误用加密端密钥当 IV，继续解密只会得到垃圾数据或填充异常）</exception>
    public static byte[] AesDecrypt(Aes aes, string text)
    {
        ArgumentNullException.ThrowIfNull(aes);

        ThrowIfAllZeroIv(aes);

        var toEncryptArray = Convert.FromBase64String(text);
        using var decrypt = aes.CreateDecryptor();
        return decrypt.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }

    /// <summary>
    /// 使用指定的 AES 实例加密明文（与 <see cref="AesDecrypt(Aes, string)"/> 配套；
    /// CBC 模式下加密完成后须从 <see cref="SymmetricAlgorithm.IV"/> 读取 IV 并随密文分发）
    /// </summary>
    /// <param name="aes">AES 加密器实例（须已设置 Key/Mode/Padding，CBC 模式下 IV 已就绪）</param>
    /// <param name="data">待加密的明文字节数组</param>
    /// <returns>加密后的密文字节数组</returns>
    /// <exception cref="ArgumentNullException">aes 或 data 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">CBC 模式下 IV 为全零时抛出（创建/加密/解密三端统一拒绝，
    /// 避免"加密成功、配套解密接口却拒绝还原"的不对称）</exception>
    public static byte[] AesEncrypt(Aes aes, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(aes);
        ArgumentNullException.ThrowIfNull(data);

        ThrowIfAllZeroIv(aes);

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    /// <summary>
    /// 使用指定的 AES 实例加密明文文本，返回 Base64 编码密文
    /// （与 <see cref="AesDecryptToString(Aes, string)"/> 配套）
    /// </summary>
    /// <param name="aes">AES 加密器实例（须已设置 Key/Mode/Padding，CBC 模式下 IV 已就绪）</param>
    /// <param name="text">待加密的明文文本</param>
    /// <returns>Base64 编码的密文</returns>
    /// <exception cref="ArgumentNullException">aes 或 text 为 null 时抛出</exception>
    public static string AesEncryptToString(Aes aes, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Convert.ToBase64String(AesEncrypt(aes, Encoding.UTF8.GetBytes(text)));
    }

    /// <summary>
    /// 使用指定的 AES 实例解密 Base64 编码密文，返回明文文本
    /// </summary>
    /// <param name="aes">AES 加密器实例（须已设置 Key/Mode/Padding，CBC 模式下 IV 与加密端一致）</param>
    /// <param name="text">Base64 编码的密文</param>
    /// <returns>明文文本</returns>
    /// <exception cref="ArgumentNullException">aes 或 text 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">CBC 模式下 IV 为全零占位值时抛出</exception>
    public static string AesDecryptToString(Aes aes, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Encoding.UTF8.GetString(AesDecrypt(aes, text));
    }
}
