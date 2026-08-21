using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Auditing.Configuration;

internal static class AuditEntityConfiguration
{
    public static void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.HasMany(x => x.Properties).WithOne(x => x.Entity);

        builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);

        // 复合主键实体的 EntityId 以 "|" 拼接可能超过 36 字符：
        // 仅当未显式设置长度时默认 36，用户已自定义长度（如 512）时以用户配置为准
        var entityIdProperty = builder.Property(x => x.EntityId);
        if (entityIdProperty.Metadata.GetMaxLength() == null)
        {
            entityIdProperty.HasMaxLength(256);
        }

        builder.Property(x => x.Type).HasMaxLength(256);
        builder.Property(e => e.OperationType).HasMaxLength(256);

        builder.HasIndex(m => m.EntityId);
    }
}
