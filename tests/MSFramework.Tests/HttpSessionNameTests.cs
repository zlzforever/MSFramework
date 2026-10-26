using System.Globalization;
using System.Security.Claims;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.IdentityModel;
using Microsoft.AspNetCore.Http;
using Xunit;
using ISession = MicroserviceFramework.Application.ISession;

namespace MSFramework.Tests;

/// <summary>验证 HTTP 会话的登录用户名与显示姓名分离，以及原始和映射后 Claims 的姓名回退顺序。</summary>
public class HttpSessionNameTests
{
    /// <summary>有完整姓名时优先显示姓名，用户名始终来自 preferred_username。</summary>
    /// <param name="mapped">是否使用 JWT 入站映射后的 Claim 类型。</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullNameTakesPrecedenceOverFamilyAndGivenNames(bool mapped)
    {
        var session = CreateSession(
            new Claim(mapped ? ClaimTypes.Name : JwtClaimTypes.Name, "完整姓名"),
            new Claim(mapped ? ClaimTypes.Surname : JwtClaimTypes.FamilyName, "张"),
            new Claim(mapped ? ClaimTypes.GivenName : JwtClaimTypes.GivenName, "三"),
            new Claim(JwtClaimTypes.PreferredUserName, "login-name"));

        Assert.Equal("完整姓名", session.UserDisplayName);
        Assert.Equal("login-name", session.UserName);
    }

    /// <summary>没有完整姓名时按中文文化拼接，缺少姓名时才回退登录用户名。</summary>
    /// <param name="family">姓，空字符串表示缺失。</param>
    /// <param name="given">名，空字符串表示缺失。</param>
    /// <param name="expected">预期显示名称。</param>
    [Theory]
    [InlineData("张", "三", "张三")]
    [InlineData("张", "", "张")]
    [InlineData("", "三", "三")]
    [InlineData("", "", "login-name")]
    public void DisplayNameFallsBackToFamilyGivenThenUserName(string family, string given, string expected)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");
            var session = CreateSession(new Claim(JwtClaimTypes.Name, ""),
                new Claim(JwtClaimTypes.FamilyName, family),
                new Claim(JwtClaimTypes.GivenName, given),
                new Claim(JwtClaimTypes.PreferredUserName, "login-name"));

            Assert.Equal(expected, session.UserDisplayName);
            Assert.Equal("login-name", session.UserName);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>姓名回退顺序随当前文化变化，不影响登录用户名。</summary>
    /// <param name="culture">当前请求文化名称。</param>
    /// <param name="expected">按文化顺序拼接后的姓名。</param>
    [Theory]
    [InlineData("zh-CN", "FamilyGiven")]
    [InlineData("zh-TW", "FamilyGiven")]
    [InlineData("en-US", "GivenFamily")]
    public void DisplayNameCompositionRespectsCurrentCulture(string culture, string expected)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var session = CreateSession(new Claim(JwtClaimTypes.FamilyName, "Family"),
                new Claim(JwtClaimTypes.GivenName, "Given"),
                new Claim(JwtClaimTypes.PreferredUserName, "login-name"));

            Assert.Equal(expected, session.UserDisplayName);
            Assert.Equal("login-name", session.UserName);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>姓名不能冒充登录用户名，无 Claims 的匿名会话保持空名称。</summary>
    [Fact]
    public void MissingPreferredUserNameDoesNotUsePersonalName()
    {
        var session = CreateSession(new Claim(ClaimTypes.Name, "姓名"));
        Assert.Null(session.UserName);
        Assert.Equal("姓名", session.UserDisplayName);
        var anonymous = CreateSession();
        Assert.Null(anonymous.UserName);
        Assert.Null(anonymous.UserDisplayName);
    }

    private static ISession CreateSession(params Claim[] claims) => HttpSession.Create(new HttpContextAccessor
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        }
    });
}
