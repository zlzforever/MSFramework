using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using MicroserviceFramework.Utils;
using Xunit;

namespace MSFramework.Tests;

public class StringTests
{
    [Fact]
    public void ToHexString()
    {
        var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("abcd"));

        var hex2 = Cryptography.ComputeMD5("abcd");

        // MemoryMarshal.Read<ulong>(bytes).TryFormat(dst, out int written, "X");
        // Assert.Equal(hex1, hex2);
    }
}
