using System;
using System.Linq;
using MicroserviceFramework;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
    public interface IA : IScopeDependency
    {
        string Id { get; }
    }

    public interface IB : ISingletonDependency
    {
        string Id { get; }
    }

    public interface IC : ITransientDependency
    {
        string Id { get; }
    }


    public class A : IA
    {
        public string Id { get; }

        public A()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class B : IB
    {
        public string Id { get; }

        public B()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class C : IC
    {
        public string Id { get; }

        public C()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class D : IA
    {
        public string Id { get; }

        public D()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class DependencyInjectionTests
    {
        [Fact]
        public void ScopeTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroserviceFramework(x => { x.UseDependencyInjectionLoader(); });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var scope1 = serviceProvider.CreateScope();
            var a1 = scope1.ServiceProvider.GetRequiredService<IA>();
            Assert.True(a1 is D);
            var id1 = a1.Id;
            var a3 = scope1.ServiceProvider.GetRequiredService<IA>();
            Assert.True(a3 is D);
            var id3 = a3.Id;
            scope1.Dispose();
            Assert.Equal(id1, id3);

            var scope2 = serviceProvider.CreateScope();
            var a2 = scope2.ServiceProvider.GetRequiredService<IA>();
            Assert.True(a2 is D);
            var id2 = a2.Id;
            scope1.Dispose();

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void MultiInjectionTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroserviceFramework(x => { x.UseDependencyInjectionLoader(); });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var scope1 = serviceProvider.CreateScope();

            var aList = scope1.ServiceProvider.GetServices<IA>();
            Assert.Equal(2, aList.Count());
        }

        [Fact]
        public void SingletonTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroserviceFramework(x => { x.UseDependencyInjectionLoader(); });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var b1 = serviceProvider.GetRequiredService<IB>();
            Assert.True(b1 is B);

            var b2 = serviceProvider.GetRequiredService<IB>();
            Assert.True(b2 is B);

            Assert.Equal(b1.Id, b2.Id);
        }

        [Fact]
        public void TransientTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroserviceFramework(x => { x.UseDependencyInjectionLoader(); });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var c1 = serviceProvider.GetRequiredService<IC>();
            Assert.True(c1 is C);

            var c2 = serviceProvider.GetRequiredService<IC>();
            Assert.True(c2 is C);

            Assert.NotEqual(c1.Id, c2.Id);
        }
    }
}