using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Ef.Auditing.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static void ApplyAuditingConfiguration(this ModelBuilder modelBuilder)
    {
        AuditOperationConfiguration.Instance.Configure(modelBuilder.Entity<AuditOperation>());
        AuditEntityConfiguration.Instance.Configure(modelBuilder.Entity<AuditEntity>());
        AuditPropertyConfiguration.Instance.Configure(modelBuilder.Entity<AuditProperty>());
    }
}
