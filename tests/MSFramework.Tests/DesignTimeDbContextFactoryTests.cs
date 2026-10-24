using System;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public class DesignTimeDbContextFactoryTests
{
    [Fact]
    public void CreateDbContextUsesRegisteredEfFactory()
    {
        var services = new ServiceCollection();
        var contextFactory = new TrackingContextFactory();
        services.AddSingleton<IDbContextFactory<TestContext>>(contextFactory);
        using var provider = services.BuildServiceProvider();
        var factory = new TestFactory(provider);

        using var context = factory.CreateDbContext([]);

        Assert.Equal(1, contextFactory.CreateCount);
    }

    [Fact]
    public void CreateDbContextRequiresEfFactoryRegistration()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var factory = new TestFactory(provider);

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext([]));

        Assert.Contains("AddDbContextFactory", exception.Message, StringComparison.Ordinal);
    }

    private sealed class TrackingContextFactory : IDbContextFactory<TestContext>
    {
        public int CreateCount { get; private set; }

        public TestContext CreateDbContext()
        {
            CreateCount++;
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new TestContext(options);
        }
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options);

    private sealed class TestFactory(IServiceProvider provider) : DesignTimeDbContextFactoryBase<TestContext>
    {
        protected override IServiceProvider GetServiceProvider() => provider;
        public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
        }
    }
}
