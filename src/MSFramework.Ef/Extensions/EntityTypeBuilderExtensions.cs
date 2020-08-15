using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Domain;

namespace MSFramework.Ef.Extensions
{
	public static class EntityTypeBuilderExtensions
	{
		public static void ConfigureCreation<T>(this EntityTypeBuilder<T> builder)
			where T : class, ICreation
		{
			builder.Property(x => x.CreationTime).HasColumnName("creation_time").UseUnixTimeSeconds();
			builder.Property(x => x.CreationUserId).HasColumnName("creation_user_id").HasMaxLength(256);
			builder.Property(x => x.CreationUserName).HasColumnName("creation_user_name").HasMaxLength(256);
		}

		public static void ConfigureModification<T>(this EntityTypeBuilder<T> builder)
			where T : class, IModification
		{
			builder.Property(x => x.ModificationTime).HasColumnName("modification_time").UseUnixTimeSeconds();
			builder.Property(x => x.ModificationUserId).HasColumnName("modification_user_id")
				.HasMaxLength(256);
			builder.Property(x => x.ModificationUserName).HasColumnName("modification_user_name")
				.HasMaxLength(256);
		}

		public static void ConfigureDeletion<T>(this EntityTypeBuilder<T> builder)
			where T : class, IDeletion
		{
			builder.Property(x => x.Deleted).HasColumnName("deleted").HasDefaultValue(false);
			builder.Property(x => x.DeletionTime).HasColumnName("deletion_time").UseUnixTimeSeconds();
			builder.Property(x => x.DeletionUserId).HasColumnName("deletion_user_id").HasMaxLength(256);
			builder.Property(x => x.DeletionUserName).HasColumnName("deletion_user_name").HasMaxLength(256);
		}
	}
}