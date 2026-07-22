using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Ef.Auditing.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
/// ModelBuilder 扩展方法，提供审计实体配置
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// 应用审计实体（AuditOperation/AuditEntity/AuditProperty）的数据库映射配置
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    public static void ApplyAuditingConfiguration(this ModelBuilder modelBuilder)
    {
        AuditOperationConfiguration.Configure(modelBuilder.Entity<AuditOperation>());
        AuditEntityConfiguration.Configure(modelBuilder.Entity<AuditEntity>());
        AuditPropertyConfiguration.Configure(modelBuilder.Entity<AuditProperty>());
    }
}
