using System;
using MicroserviceFramework.Extensions;
using Xunit;

namespace MSFramework.Tests;

public class DateTimeTests
{
    [Fact]
    public void ToUnixTimeSeconds()
    {
        var unixTime = 1689262630;
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        Assert.Equal(DateTime.Parse("2023-07-13 23:37:10"), dt);

        var a = DateTime.Parse("2023-07-13 23:37:10").ToUnixTimeSeconds();
        Assert.Equal(unixTime, a);
    }

    [Fact]
    public void ToLocal()
    {
        var unixTime = 1689262630;
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        var local1 = dt.ToLocalTime();
        var local2 = local1.ToLocalTime();
        var local3 = local2.ToLocalTime();
        Assert.Equal(local1, local2);
        Assert.Equal(local1, local3);
    }
}
