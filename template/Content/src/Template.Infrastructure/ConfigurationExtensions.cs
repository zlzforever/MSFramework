using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Domain;
using MSFramework.Ef.Extensions;

namespace Template.Infrastructure
{
	public static class ConfigurationExtensions
	{
		public static void ConfigureCreationAudited<T>(this EntityTypeBuilder<T> builder)
			where T : class, ICreationAudited
		{
			builder.Property(x => x.CreationTime).HasColumnName("creation_time").UseUnixTimeSeconds();;
			builder.Property(x => x.CreationUserId).HasColumnName("creation_user_id").HasMaxLength(256);
			builder.Property(x => x.CreationUserName).HasColumnName("creation_user_name").HasMaxLength(256);
		}

		public static void ConfigureModificationAudited<T>(this EntityTypeBuilder<T> builder)
			where T : class, IModificationAudited
		{
			builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time").UseUnixTimeSeconds();;
			builder.Property(x => x.LastModificationUserId).HasColumnName("last_modification_user_id")
				.HasMaxLength(256);
			builder.Property(x => x.LastModificationUserName).HasColumnName("last_modification_user_name")
				.HasMaxLength(256);
		}

		public static void ConfigureDeletionAudited<T>(this EntityTypeBuilder<T> builder)
			where T : class, IDeletionAudited
		{
			builder.Property(x => x.Deleted).HasColumnName("deleted");
			builder.Property(x => x.DeletionTime).HasColumnName("deletion_time").UseUnixTimeSeconds();;
			builder.Property(x => x.DeletionUserId).HasColumnName("deletion_user_id").HasMaxLength(256);
			builder.Property(x => x.DeletionUserName).HasColumnName("deletion_user_name").HasMaxLength(256);
		}
	}
}