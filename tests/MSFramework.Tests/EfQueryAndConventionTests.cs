using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public class EfQueryAndConventionTests
{
    [Fact]
    public async Task RepositoryDoesNotAutoIncludeNavigationsByDefault()
    {
        using var host = CreateHost();
        var context = host.Provider.GetRequiredService<TestContext>();
        context.Parents.Add(new Parent(1) { Child = new Child { Id = 2, ParentId = 1 } });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new EfRepository<Parent, int>(new DbContextFactory(host.Provider));
        var parent = await repository.FindAsync(1);

        Assert.NotNull(parent);
        Assert.Null(parent.Child);
    }

    [Fact]
    public void OwnedAndKeylessPropertiesDoNotReceiveAggregateConventions()
    {
        using var host = CreateHost();
        var context = host.Provider.GetRequiredService<TestContext>();

        var ownedProperty = context.Model.FindEntityType(typeof(OwnedDetails))!
            .FindProperty(nameof(OwnedDetails.ExternalId))!;
        var keylessProperty = context.Model.FindEntityType(typeof(QueryRow))!
            .FindProperty(nameof(QueryRow.ExternalId))!;

        Assert.NotEqual("varchar", ownedProperty.GetColumnType());
        Assert.Null(ownedProperty.GetMaxLength());
        Assert.NotEqual("varchar", keylessProperty.GetColumnType());
        Assert.Null(keylessProperty.GetMaxLength());
    }

    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { DatabaseType = "Sqlite", UseUnderScoreCase = true });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestFinder());
        services.AddDbContext<TestContext>(options => options.UseSqlite(connection));
        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<TestContext>().Database.EnsureCreated();
        return new TestHost(provider, connection);
    }

    private sealed class TestScopeServiceProvider(IServiceProvider provider) : IScopeServiceProvider
    {
        public T GetService<T>() => provider.GetService<T>();
    }

    private sealed class TestHost(ServiceProvider provider, SqliteConnection connection) : IDisposable
    {
        public ServiceProvider Provider { get; } = provider;

        public void Dispose()
        {
            Provider.Dispose();
            connection.Dispose();
        }
    }

    private sealed class TestFinder : IEntityConfigurationTypeFinder
    {
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];
        public Type GetDbContextTypeForEntity(Type entityType) => typeof(TestContext);
        public IEnumerable<Type> GetAllDbContextTypes() => [typeof(TestContext)];
        public bool HasDbContextForEntity<T>() => true;
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContextBase(options)
    {
        public DbSet<Parent> Parents => Set<Parent>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.OwnsOne(x => x.Details);
                builder.HasOne(x => x.Child).WithOne().HasForeignKey<Child>(x => x.ParentId);
            });
            modelBuilder.Entity<Child>().HasKey(x => x.Id);
            modelBuilder.Entity<QueryRow>().HasNoKey().ToView("query_rows");
        }
    }

    private sealed class Parent(int id) : EntityBase<int>(id), IAggregateRoot<int>
    {
        public Child Child { get; set; }
        public OwnedDetails Details { get; set; } = new();
    }

    private sealed class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
    }

    private sealed class OwnedDetails
    {
        public Guid ExternalId { get; set; }
    }

    private sealed class QueryRow
    {
        public Guid ExternalId { get; set; }
    }
}
