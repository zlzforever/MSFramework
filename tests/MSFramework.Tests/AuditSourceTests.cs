using System;
using System.Collections.Generic;
using System.Text.Json;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>审计来源强类型回归，验证创建入口、默认 JSON 转换及 EF 存量标识映射，不访问外部服务。</summary>
public class AuditSourceTests
{
    /// <summary>HTTP 和本地事件入口分别使用固定来源，不将来源写入 HTTP 方法。</summary>
    [Fact]
    public void CreationEntrypointsUseTypedSources()
    {
        var http = new AuditOperation("/orders", null, null, null, null, null, null, "trace", "POST");
        var local = AuditOperation.CreateForLocalEvent("Handler", "trace");

        Assert.Same(AuditSource.Http, http.Source);
        Assert.Equal("POST", http.Method);
        Assert.Same(AuditSource.LocalEvent, local.Source);
        Assert.Null(local.Method);
        Assert.Collection(Enumeration.GetAll(typeof(AuditSource)),
            source => Assert.IsType<AuditSource>(source), source => Assert.IsType<AuditSource>(source));
    }

    /// <summary>默认 JSON 转换保留历史来源字符串，反序列化返回共享枚举实例。</summary>
    /// <param name="id">历史来源标识，必须为 Http 或 Local。</param>
    [Theory]
    [InlineData("Http")]
    [InlineData("Local")]
    public void JsonPreservesExistingIdentifiers(string id)
    {
        var options = new JsonSerializerOptions();
        options.AddDefaultConverters();
        var source = Enumeration.Parse(typeof(AuditSource), id);

        Assert.Equal($"\"{id}\"", JsonSerializer.Serialize((AuditSource)source, options));
        Assert.Same(source, JsonSerializer.Deserialize<AuditSource>($"\"{id}\"", options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<AuditSource>("\"Unknown\"", options));
    }

    /// <summary>实际框架模型仍将来源映射为可空的 16 字符列，转换器双向兼容存量标识。</summary>
    [Fact]
    public void EfConventionPreservesSourceColumnAndValues()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new EmptyConfigurationFinder());
        services.AddDbContext<AuditContext>(options => options.UseSqlite("Data Source=:memory:"));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
        var property = context.Model.FindEntityType(typeof(AuditOperation))?.FindProperty(nameof(AuditOperation.Source));

        Assert.NotNull(property);
        Assert.Equal(typeof(AuditSource), property.ClrType);
        Assert.Equal(16, property.GetMaxLength());
        Assert.True(property.IsNullable);
        var converter = property.GetValueConverter();
        Assert.NotNull(converter);
        Assert.Equal("Http", converter.ConvertToProvider(AuditSource.Http));
        Assert.Equal("Local", converter.ConvertToProvider(AuditSource.LocalEvent));
        Assert.Same(AuditSource.Http, converter.ConvertFromProvider("Http"));
        Assert.Same(AuditSource.LocalEvent, converter.ConvertFromProvider("Local"));
    }

    private sealed class AuditContext(DbContextOptions<AuditContext> options) : DbContextBase(options)
    {
        /// <summary>注册框架审计模型，以验证真实枚举约定。</summary>
        /// <param name="modelBuilder">EF 模型构建器，不允许为空。</param>
        /// <exception cref="ArgumentNullException">构建器为空。</exception>
        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyAuditingConfiguration();
        }
    }

    private sealed class EmptyConfigurationFinder : IEntityConfigurationTypeFinder
    {
        /// <inheritdoc/>
        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        /// <inheritdoc/>
        public Type GetDbContextTypeForEntity(Type entityType) => null;

        /// <inheritdoc/>
        public IEnumerable<Type> GetAllDbContextTypes() => [typeof(AuditContext)];

        /// <inheritdoc/>
        public bool HasDbContextForEntity<T>() => false;
    }
}
