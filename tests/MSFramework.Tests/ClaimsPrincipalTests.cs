using System.Security.Claims;
using MicroserviceFramework.Security.Claims;
using Xunit;

namespace MSFramework.Tests;

public class ClaimsPrincipalTests
{
    [Fact]
    public void GetUserId()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
        var userId = identity.GetUserId();
        Assert.Equal("1", userId);
        ClaimsIdentity identity2 = null;
        Assert.Null(identity2.GetUserId());

        var identity3 = new ClaimsIdentity();
        Assert.Null(identity3.GetUserId());
    }

    [Fact]
    public void GetValue()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
        identity.AddClaim(new Claim(ClaimTypes.Email, "1@163.com"));
        var p = new ClaimsPrincipal();
        p.AddIdentity(identity);
        Assert.Equal("1", p.GetValue(ClaimTypes.NameIdentifier));
        Assert.Equal("1@163.com", p.GetValue(ClaimTypes.Email));
        Assert.Equal("1@163.com", p.GetValue(ClaimTypes.Email, ClaimTypes.NameIdentifier));
        Assert.Equal("1", p.GetValue(ClaimTypes.NameIdentifier, ClaimTypes.Email));
    }
}
