using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureCreation<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ICreation
    {
        builder.Property(x => x.CreationTime).UseUnixTime();
        builder.Property(x => x.CreatorId).HasMaxLength(36);
        builder.Property(x => x.CreatorName).HasMaxLength(255);

        // comments: 是否需要索引要由业务方来指定
        // builder.HasIndex(x => x.CreationTime);
    }

    public static void ConfigureModification<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IModification
    {
        builder.Property(x => x.LastModificationTime).UseUnixTime();
        builder.Property(x => x.LastModifierId).HasMaxLength(36);
        builder.Property(x => x.LastModifierName).HasMaxLength(255);

        // comments: 是否需要索引要由业务方来指定
        // builder.HasIndex(x => x.LastModificationTime);
    }

    public static void ConfigureDeletion<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IDeletion
    {
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletionTime).UseUnixTime();
        builder.Property(x => x.DeleterId).HasMaxLength(36);
        builder.Property(x => x.DeleterName).HasMaxLength(255);

        // comments: 是否需要索引要由业务方来指定
        // builder.HasIndex(x => x.DeletionTime);
    }
}
