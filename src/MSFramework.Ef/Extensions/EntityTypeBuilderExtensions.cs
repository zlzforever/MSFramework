using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions
{
	public static class EntityTypeBuilderExtensions
	{
		public static void ConfigureConcurrencyStamp<TEntity>(this EntityTypeBuilder<TEntity> b)
			where TEntity : class, IOptimisticLock
		{
			b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken().HasMaxLength(36);
		}

		public static void ConfigureCreation<TEntity>(this EntityTypeBuilder<TEntity> builder)
			where TEntity : class, ICreation
		{
			builder.Property(x => x.CreationTime).UseUnixTime();
			builder.Property(x => x.CreatorId).HasMaxLength(255);

			builder.HasIndex(x => x.CreationTime);
		}

		public static void ConfigureModification<TEntity>(this EntityTypeBuilder<TEntity> builder)
			where TEntity : class, IModification
		{
			builder.Property(x => x.LastModificationTime).UseUnixTime();
			builder.Property(x => x.LastModifierId).HasMaxLength(255);

			builder.HasIndex(x => x.LastModificationTime);
		}

		public static void ConfigureDeletion<TEntity>(this EntityTypeBuilder<TEntity> builder)
			where TEntity : class, IDeletion
		{
			builder.Property(x => x.IsDeleted).HasDefaultValue(false);
			builder.Property(x => x.DeletionTime).UseUnixTime();
			builder.Property(x => x.DeleterId).HasMaxLength(255);

			builder.HasIndex(x => x.DeletionTime);
		}
	}
}