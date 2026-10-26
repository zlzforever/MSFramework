using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 物理删除回归测试：
/// 验证未实现任何审计接口（ICreation/IModification/IDeletion）的实体
/// （如复合主键多列实体 <c>MultiColumnOrderItem : EntityBase, IAggregateRoot</c>），
/// 在保存时不会被任何审计策略命中（Deleted 状态下 CreationAuditingStrategy 只认 Added、
/// DeletionAuditingStrategy 只认 IDeletion、ModificationAuditingStrategy 只认 Modified），
/// 物理删除变更仍必须提交，不能被"以 ApplyConcepts 返回值作为保存门禁"的旧实现静默丢弃。
/// 该门禁已解耦为 <see cref="ChangeTracker.HasChanges()"/>（DbContextBase），本测试锁定该行为。
/// </summary>
public class PhysicalDeleteTests
{
    /// <summary>
    /// 无审计接口的实体：仅继承 <see cref="EntityBase{TKey}"/>，无 ICreation/IModification/IDeletion
    /// </summary>
    public class AuditLessEntity : EntityBase<int>
    {
        public AuditLessEntity(int id) : base(id)
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 测试用 DbContext
    /// </summary>
    public class PhysicalDeleteContext(DbContextOptions<PhysicalDeleteContext> options) : DbContextBase(options)
    {
        public DbSet<AuditLessEntity> AuditLessEntities => Set<AuditLessEntity>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLessEntity>(builder => builder.Property(x => x.Id).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(AuditLessEntity)] = typeof(PhysicalDeleteContext)
        };

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() =>
            EntityToDbContext.Values.Distinct();

        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 测试用作用域服务提供程序
    /// </summary>
    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接与服务容器，保证连接在测试生命周期内存活
    /// </summary>
    private sealed class TestHost : IDisposable
    {
        public TestHost(ServiceProvider provider, SqliteConnection connection)
        {
            Provider = provider;
            _connection = connection;
        }

        public ServiceProvider Provider { get; }

        private readonly SqliteConnection _connection;

        public void Dispose()
        {
            _connection.Dispose();
            Provider.Dispose();
        }
    }

    /// <summary>
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库）、设置、实体查找器与作用域提供程序
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<PhysicalDeleteContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<PhysicalDeleteContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 物理删除（无审计接口实体）必须真正提交：旧实现中 Deleted 且未实现 IDeletion 的实体
    /// 不会被任何审计策略命中，ApplyConcepts 返回 false 导致整次保存被跳过、删除静默丢失
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_PhysicalDeleteAuditLessEntity_CommitsDeletion()
    {
        using var host = CreateHost();
        var context = host.Provider.GetRequiredService<PhysicalDeleteContext>();

        // Arrange：先插入一条无审计接口的实体
        var entity = new AuditLessEntity(1) { Name = "to-delete" };
        context.AuditLessEntities.Add(entity);
        Assert.Equal(1, await context.SaveChangesAsync());

        // Act：物理删除（任何审计策略都不命中）
        context.AuditLessEntities.Remove(entity);
        var deleted = await context.SaveChangesAsync();

        // Assert：删除必须真正落库，而不是被静默丢弃
        Assert.Equal(1, deleted);
        Assert.False(await context.AuditLessEntities.AnyAsync(x => x.Id == 1));
    }

    /// <summary>
    /// 无任何变更时保存返回 0，不抛异常（门禁解耦后的基线行为）
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_NoChanges_ReturnsZero()
    {
        using var host = CreateHost();
        var context = host.Provider.GetRequiredService<PhysicalDeleteContext>();

        // Act：无任何变更时保存
        var effected = await context.SaveChangesAsync();

        // Assert：返回 0 且不抛异常
        Assert.Equal(0, effected);
    }
}
