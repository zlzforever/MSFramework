using System.Collections.Generic;
using MicroserviceFramework.Collections.Generic;
using Xunit;

// using MicroserviceFramework.Collections.Generic;

namespace MSFramework.Tests;

public class EnumerableExtensionsTests
{
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
}
