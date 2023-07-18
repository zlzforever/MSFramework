using System;
using Xunit;
using MicroserviceFramework.Runtime;

namespace MSFramework.Tests;

public class TypeTests
{
    interface IInterface
    {
    }

    public class A : IInterface
    {
    }

    public class B : A
    {
    }

    interface IG<T>
    {
    }

    public class C : IG<int>
    {
    }

    public class D : IG<D>
    {
    }

    [Fact]
    public void GetInterfaces()
    {
        var interfaces0 = typeof(A).GetInterfacesExcludeBy(null);
        Assert.Equal(typeof(IInterface), interfaces0[0]);

        var interfaces1 = typeof(A).GetInterfacesExcludeBy(Type.EmptyTypes);
        Assert.Equal(typeof(IInterface), interfaces1[0]);

        var interfaces11 = typeof(A).GetInterfacesExcludeBy(typeof(IInterface));
        Assert.Empty(interfaces11);

        var interfaces2 = typeof(B).GetInterfacesExcludeBy(Type.EmptyTypes);
        Assert.Equal(typeof(IInterface), interfaces2[0]);

        var interfaces21 = typeof(B).GetInterfacesExcludeBy(typeof(IInterface));
        Assert.Empty(interfaces21);

        var interfaces3 = typeof(C).GetInterfacesExcludeBy(Type.EmptyTypes);
        Assert.Equal(typeof(IG<int>), interfaces3[0]);

        var interfaces4 = typeof(D).GetInterfacesExcludeBy(Type.EmptyTypes);
        Assert.Equal(typeof(IG<D>), interfaces4[0]);
    }
}
