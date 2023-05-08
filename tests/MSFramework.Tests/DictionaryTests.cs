using System.Collections.Generic;
using MicroserviceFramework.Collections.Generic;
using Xunit;

namespace MSFramework.Tests;

public class DictionaryTests
{
    [Fact]
    public void AddOrUpdate()
    {
        var dict = new Dictionary<string, string>();
        dict.AddOrUpdate("a", "b");
        Assert.Equal("b", dict["a"]);
        dict.AddOrUpdate("a", "b1");
        Assert.Equal("b1", dict["a"]);
        dict.AddOrUpdate("a", () => "b2");
        Assert.Equal("b2", dict["a"]);
    }

    [Fact]
    public void GetOrDefault()
    {
        var dict = new Dictionary<string, string>();
        dict.AddOrUpdate("a", "b");
        Assert.Equal("b", dict.GetOrDefault("a"));
        Assert.Null(dict.GetOrDefault("aaa"));
    }

    [Fact]
    public void GetOrAdd()
    {
        var dict = new Dictionary<string, string>();
        dict.GetOrAdd("a", "b");
        Assert.Equal("b", dict.GetOrDefault("a"));
    }
}
