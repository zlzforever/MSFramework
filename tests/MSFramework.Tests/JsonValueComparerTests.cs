using System;
using System.Collections.Generic;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Internal;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MSFramework.Tests;

public class JsonValueComparerTests
{
    [Fact]
    public void JsonPropertyComparerIgnoresObjectKeyOrder()
    {
        var modelBuilder = new ModelBuilder();
        var property = modelBuilder.Entity<JsonEntity>().Property(x => x.Payload);
        property.UseJson();
        var comparer = property.Metadata.GetValueComparer()!;
        var left = new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 };
        var right = new Dictionary<string, int> { ["beta"] = 2, ["alpha"] = 1 };

        Assert.True(comparer.Equals(left, right));
        Assert.Equal(comparer.GetHashCode(left), comparer.GetHashCode(right));
    }

    [Fact]
    public void StreamComparerIgnoresObjectKeyOrder()
    {
        var comparer = new JsonStreamValueComparer<Dictionary<string, int>>();
        var left = new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 };
        var right = new Dictionary<string, int> { ["beta"] = 2, ["alpha"] = 1 };

        Assert.True(comparer.Equals(left, right));
        Assert.Equal(comparer.GetHashCode(left), comparer.GetHashCode(right));
    }

    [Fact]
    public void UseJsonRejectsIncompatibleFieldTypeDuringModelConfiguration()
    {
        var modelBuilder = new ModelBuilder();
        var property = modelBuilder.Entity<JsonEntity>().Property(x => x.Payload);

        var exception = Assert.Throws<ArgumentException>(() =>
            property.UseJson(typeof(List<int>)));

        Assert.Equal("fieldType", exception.ParamName);
    }

    private sealed class JsonEntity
    {
        public int Id { get; set; }
        public Dictionary<string, int> Payload { get; set; } = [];
    }
}
