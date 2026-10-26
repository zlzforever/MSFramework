using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class CryptographyTests
{
    [Fact]
    public async Task ComputeMD5()
    {
        var md51 = Cryptography.ComputeMD5("123");
        var data = "123"u8.ToArray();
        var md52 = Cryptography.ComputeMD5(data);
        var md53 = await Cryptography.ComputeMD5Async(new MemoryStream(data));
        Assert.Equal("202CB962AC59075B964B07152D234B70", md51);
        Assert.Equal("202CB962AC59075B964B07152D234B70", md52);
        Assert.Equal("202CB962AC59075B964B07152D234B70", md53);
    }

    private const string AesKey = "0123456789abcdef"; // 16 字节（AES-128）

    private static readonly byte[] AesIv =
        [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10];

    [Fact]
    public void CreateAes_NullOrEmptyKey_Throws()
    {
        // Check.NotNullOrEmpty 抛出 ArgumentException（带参数名）
        Assert.Throws<ArgumentException>(() => Cryptography.CreateAes(null, AesIv, CipherMode.CBC));
        Assert.Throws<ArgumentException>(() => Cryptography.CreateAes(string.Empty, AesIv, CipherMode.CBC));
    }

    [Fact]
    public void CreateAes_InvalidKeyLength_Throws()
    {
        // "123" 的 UTF-8 为 3 字节，不符合 AES-128/192/256 的 16/24/32 字节要求
        Assert.Throws<ArgumentException>(() => Cryptography.CreateAes("123", AesIv, CipherMode.CBC));
    }

    [Fact]
    public void CreateAes_CbcWithoutIv_Throws()
    {
        // CBC 强制显式 IV：自动生成的随机 IV 在解密端独立构造时会静默解出垃圾数据
        Assert.Throws<ArgumentException>(() => Cryptography.CreateAes(AesKey, iv: null, CipherMode.CBC));
    }

    [Fact]
    public void CreateAes_EcbWithIv_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Cryptography.CreateAes(AesKey, AesIv, CipherMode.ECB));
    }

    [Fact]
    public void CreateAes_IvWrongLength_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Cryptography.CreateAes(AesKey, new byte[15], CipherMode.CBC));
    }

    [Fact]
    public void Aes_CbcExplicitIv_RoundTrip()
    {
        var encrypted = Cryptography.AesEncryptToString(
            Cryptography.CreateAes(AesKey, AesIv, CipherMode.CBC), "机密数据");
        var decrypted = Cryptography.AesDecryptToString(
            Cryptography.CreateAes(AesKey, AesIv, CipherMode.CBC), encrypted);
        Assert.Equal("机密数据", decrypted);
    }

    [Fact]
    public void Aes_CbcAllZeroIv_IsRejectedSymmetrically()
    {
        // 全零 IV 三端统一拒绝（创建/加密/解密同一策略）：
        // 若只拒绝解密端，会出现"加密成功、配套解密接口却拒绝还原"的不对称
        Assert.Throws<ArgumentException>(() =>
            Cryptography.CreateAes(AesKey, new byte[16], CipherMode.CBC));

        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(AesKey);
        aes.IV = new byte[16];
        aes.Mode = CipherMode.CBC;
        Assert.Throws<ArgumentException>(
            () => Cryptography.AesEncrypt(aes, new byte[32]));
        Assert.Throws<ArgumentException>(
            () => Cryptography.AesDecrypt(aes, Convert.ToBase64String(new byte[32])));
    }

    [Fact]
    public void Aes_LegacyEcbPath_RoundTrip()
    {
        // 历史 ECB 契约：兼容重载默认 ECB，加解密回环（兼容存量密文）
#pragma warning disable CS0618
        using var legacyAes = Cryptography.CreateAes(AesKey);
#pragma warning restore CS0618
        Assert.Equal(CipherMode.ECB, legacyAes.Mode);

        var encrypted = Cryptography.AesEncryptToString(legacyAes, "legacy");
        var decrypted = Cryptography.AesDecryptToString(encrypted, AesKey);
        Assert.Equal("legacy", decrypted);
    }
}
