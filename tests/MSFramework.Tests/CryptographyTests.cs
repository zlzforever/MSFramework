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

    [Fact]
    public void AesEncryptToStringRoundTripsWithAuthenticatedFormat()
    {
        var encrypted = Cryptography.AesEncryptToString("sensitive payload", "correct horse battery staple");

        Assert.StartsWith("msf:aesgcm:v1:", encrypted);
        Assert.Equal("sensitive payload",
            Cryptography.AesDecryptToString(encrypted, "correct horse battery staple"));
    }

    [Fact]
    public void AesEncryptUsesUniqueNonceForEveryMessage()
    {
        var first = Cryptography.AesEncryptToString("same payload", "correct horse battery staple");
        var second = Cryptography.AesEncryptToString("same payload", "correct horse battery staple");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void AesDecryptRejectsTamperingAndWrongPassword()
    {
        var encrypted = Cryptography.AesEncryptToString("sensitive payload", "correct horse battery staple");
        var payload = Convert.FromBase64String(encrypted["msf:aesgcm:v1:".Length..]);
        payload[^1] ^= 0x01;
        var tampered = "msf:aesgcm:v1:" + Convert.ToBase64String(payload);

        Assert.Throws<AuthenticationTagMismatchException>(() =>
            Cryptography.AesDecryptToString(tampered, "correct horse battery staple"));
        Assert.Throws<AuthenticationTagMismatchException>(() =>
            Cryptography.AesDecryptToString(encrypted, "wrong password"));
    }

    [Fact]
    public void AesDecryptReadsLegacyEcbCiphertextForMigration()
    {
#pragma warning disable CS0618
        using var aes = Cryptography.CreateAes("0123456789abcdef");
#pragma warning restore CS0618
        using var encryptor = aes.CreateEncryptor();
        var plaintext = Encoding.UTF8.GetBytes("legacy payload");
        var ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        var decrypted = Cryptography.AesDecryptToString(
            Convert.ToBase64String(ciphertext), "0123456789abcdef");

        Assert.Equal("legacy payload", decrypted);
    }
}
