using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public sealed class UnitOfWorkAtomicityTests
{
    private sealed class FirstEntity(string id) : EntityBase<string>(id);
    private sealed class SecondEntity(string id, string name) : EntityBase<string>(id)
    {
        public string Name { get; set; } = name;
    }

    private sealed class FirstContext(DbContextOptions<FirstContext> options) : DbContextBase(options)
    {
        public DbSet<FirstEntity> Items => Set<FirstEntity>();
        protected override void ApplyConfiguration(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<FirstEntity>().HasKey(x => x.Id);
    }

    private sealed class SecondContext(DbContextOptions<SecondContext> options) : DbContextBase(options)
    {
        public DbSet<SecondEntity> Items => Set<SecondEntity>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecondEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<SecondEntity>().HasIndex(x => x.Name).IsUnique();
        }
    }

    private sealed class Finder : IEntityConfigurationTypeFinder
    {
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];
        public Type GetDbContextTypeForEntity(Type entityType) =>
            entityType == typeof(FirstEntity) ? typeof(FirstContext) : typeof(SecondContext);
        public IEnumerable<Type> GetAllDbContextTypes() => [typeof(FirstContext), typeof(SecondContext)];
        public bool HasDbContextForEntity<T>() => true;
    }

    private sealed class EmptyScopeProvider : IScopeServiceProvider
    {
        public T GetService<T>() => default;
    }

    [Fact]
    public async Task SaveChanges_SameConnectionRollsBackAllContextsWhenOneFails()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var provider = CreateProvider(connection, connection);
        var first = provider.GetRequiredService<FirstContext>();
        var second = provider.GetRequiredService<SecondContext>();
        await first.Database.EnsureCreatedAsync();
        await second.Database.EnsureCreatedAsync();
        first.Items.Add(new FirstEntity("first"));
        second.Items.AddRange(new SecondEntity("one", "same"), new SecondEntity("two", "same"));

        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());

        first.ChangeTracker.Clear();
        Assert.Equal(0, await first.Items.CountAsync());
    }

    [Fact]
    public async Task SaveChanges_DifferentConnectionsRejectsBeforeSaving()
    {
        await using var firstConnection = new SqliteConnection("DataSource=:memory:");
        await using var secondConnection = new SqliteConnection("DataSource=:memory:");
        await firstConnection.OpenAsync();
        await secondConnection.OpenAsync();
        await using var provider = CreateProvider(firstConnection, secondConnection);
        var first = provider.GetRequiredService<FirstContext>();
        var second = provider.GetRequiredService<SecondContext>();
        await first.Database.EnsureCreatedAsync();
        await second.Database.EnsureCreatedAsync();
        first.Items.Add(new FirstEntity("first"));
        second.Items.Add(new SecondEntity("second", "second"));

        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.SaveChangesAsync());

        Assert.Contains("same database connection", error.Message, StringComparison.OrdinalIgnoreCase);
        first.ChangeTracker.Clear();
        Assert.Equal(0, await first.Items.CountAsync());
    }

    [Fact]
    public async Task SaveChanges_InvokesLegacyAndAsyncCallbacksAfterCommit()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var provider = CreateProvider(connection, connection);
        var first = provider.GetRequiredService<FirstContext>();
        await first.Database.EnsureCreatedAsync();
        first.Items.Add(new FirstEntity("first"));
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var callbacks = new List<string>();
        unitOfWork.SavedChanges += () => callbacks.Add("legacy");
        unitOfWork.SavedChangesAsync += _ =>
        {
            callbacks.Add("async");
            return ValueTask.CompletedTask;
        };

        await unitOfWork.SaveChangesAsync();

        Assert.Equal(["legacy", "async"], callbacks);
    }

    private static ServiceProvider CreateProvider(SqliteConnection first, SqliteConnection second)
    {
        var services = new ServiceCollection();
        services.AddMicroserviceFramework();
        services.AddSingleton(new DbContextSettings { DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new Finder());
        services.AddSingleton<IScopeServiceProvider, EmptyScopeProvider>();
        services.AddDbContext<FirstContext>(options => options.UseSqlite(first));
        services.AddDbContext<SecondContext>(options => options.UseSqlite(second));
        services.AddEntityFrameworkExtension();
        return services.BuildServiceProvider();
    }
}
