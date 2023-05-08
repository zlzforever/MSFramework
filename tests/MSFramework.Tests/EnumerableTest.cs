using System.Collections.Generic;
using MicroserviceFramework.Collections.Generic;
using Xunit;

namespace MSFramework.Tests;

public class EnumerableTest
{
    [Fact]
    public void IsEmpty()
    {
        List<int> list1 = null;
        var a = list1.IsNullOrEmpty();
        Assert.True(a);
    }

    class A
    {
        public string Name { get; set; }
    }

    [Fact]
    public void Join()
    {
        var list = new List<string> { "1", "2" };
        var str = list.JoinString(", ");
        Assert.Equal("1, 2", str);

        var list2 = new List<A> { new A { Name = "1" }, new A { Name = "2" } };
        var str2 = list2.JoinString(", ", x => x.Name);
        Assert.Equal("1, 2", str2);
    }

    [Fact]
    public void HasDuplicates()
    {
        var list = new List<A> { new A { Name = "1" }, new A { Name = "2" }, new A { Name = "2" } };
        var list2 = new List<A> { new A { Name = "1" }, new A { Name = "2" } };
        Assert.True(list.HasDuplicates(t => t.Name));
        Assert.False(list2.HasDuplicates(t => t.Name));
    }
}
