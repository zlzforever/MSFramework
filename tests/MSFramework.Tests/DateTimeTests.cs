using System;
using MicroserviceFramework.Extensions;
using Xunit;

namespace MSFramework.Tests;

public class DateTimeTests
{
    [Fact]
    public void A()
    {
        var unixTime = 1689262630;
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        Assert.Equal(DateTime.Parse("2023-07-13 23:37:10"), dt);

        var a = DateTime.Parse("2023-07-13 23:37:10").ToUnixTimeSeconds();
        Assert.Equal(unixTime, a);
    }
}
