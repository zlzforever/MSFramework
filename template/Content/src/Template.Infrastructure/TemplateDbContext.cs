using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure;

/// <summary>唯一可写上下文，依赖带框架设置的选项，集中保存业务实体、领域事件与审计模型。</summary>
/// <param name="options">由 MySQL Provider 和 Load(settings) 注册的类型化选项。</param>
public class TemplateDbContext(DbContextOptions<TemplateDbContext> options) : DbContextBase(options)
{
    /// <summary>注册审计模型；业务映射由框架扫描 EntityTypeConfigurationBase 自动加载。</summary>
    /// <param name="modelBuilder">EF 模型构建器，不允许为空。</param>
    /// <exception cref="ArgumentNullException">模型构建器为空。</exception>
    protected override void ApplyConfiguration(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyAuditingConfiguration();
    }
}
