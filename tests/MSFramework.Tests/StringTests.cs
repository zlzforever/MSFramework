using System.Security.Cryptography;
using System.Text;
using MicroserviceFramework.Runtime;
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

    [Fact]
    public void IsNullOrEmpty_ReturnsTrue_ForNullString()
    {
        string value = null;
        var result = value.IsNullOrEmpty();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsTrue_ForEmptyString()
    {
        var value = "";
        var result = value.IsNullOrEmpty();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsFalse_ForNonEmptyString()
    {
        var value = "test";
        var result = value.IsNullOrEmpty();
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_ReturnsTrue_ForNullString()
    {
        string value = null;
        var result = value.IsNullOrWhiteSpace();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_ReturnsTrue_ForEmptyString()
    {
        var value = "";
        var result = value.IsNullOrWhiteSpace();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_ReturnsTrue_ForWhiteSpaceString()
    {
        var value = "   ";
        var result = value.IsNullOrWhiteSpace();
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_ReturnsFalse_ForNonWhiteSpaceString()
    {
        var value = "test";
        var result = value.IsNullOrWhiteSpace();
        Assert.False(result);
    }

    [Fact]
    public void ToSnakeCase_ConvertsCamelCaseToSnakeCase()
    {
        var value = "camelCaseString";
        var result = value.ToSnakeCase();
        Assert.Equal("camel_case_string", result);
    }

    [Fact]
    public void ToSnakeCase_HandlesEmptyString()
    {
        var value = "";
        var result = value.ToSnakeCase();
        Assert.Equal("", result);
    }

    [Fact]
    public void ToCamelCase_ConvertsPascalCaseToCamelCase()
    {
        var value = "PascalCaseString";
        var result = value.ToCamelCase();
        Assert.Equal("pascalCaseString", result);
    }

    [Fact]
    public void ToCamelCase_HandlesEmptyString()
    {
        var value = "";
        var result = value.ToCamelCase();
        Assert.Equal("", result);
    }

    [Fact]
    public void ToCamelCase_HandlesSingleCharacterString()
    {
        var value = "A";
        var result = value.ToCamelCase();
        Assert.Equal("a", result);
    }
}
