using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroserviceFramework.Ef.Extensions
{
	public static class EntityTypeBuilderExtensions
	{
		public static void ConfigureCreation<T>(this EntityTypeBuilder<T> builder)
			where T : class, ICreation
		{
			builder.Property(x => x.CreationTime).UseUnixTimeSeconds();
			builder.Property(x => x.CreationUserId).HasMaxLength(255);
			builder.Property(x => x.CreationUserName).HasMaxLength(255);
		}

		public static void ConfigureModification<T>(this EntityTypeBuilder<T> builder)
			where T : class, IModification
		{
			builder.Property(x => x.ModificationTime).UseUnixTimeSeconds();
			builder.Property(x => x.ModificationUserId).HasMaxLength(255);
			builder.Property(x => x.ModificationUserName).HasMaxLength(255);
		}

		public static void ConfigureDeletion<T>(this EntityTypeBuilder<T> builder)
			where T : class, IDeletion
		{
			builder.Property(x => x.Deleted).HasDefaultValue(false);
			builder.Property(x => x.DeletionTime).UseUnixTimeSeconds();
			builder.Property(x => x.DeletionUserId).HasMaxLength(255);
			builder.Property(x => x.DeletionUserName).HasMaxLength(255);
		}
	}
}