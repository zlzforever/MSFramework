using Xunit;

namespace MSFramework.Tests;

public class ClassTests
{
    private abstract class A
    {
    }

    [Fact]
    public void AbstractClass()
    {
        var r1 = typeof(A).IsAbstract;
        var r2 = typeof(A).IsClass;

        Assert.True(r1);
        Assert.True(r2);
    }
}
