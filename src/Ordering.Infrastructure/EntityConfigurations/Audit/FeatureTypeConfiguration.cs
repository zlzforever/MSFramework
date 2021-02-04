using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.FeatureManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.EntityConfigurations.Audit
{
	public class FeatureTypeConfiguration : EntityTypeConfigurationBase<Feature, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<Feature> builder)
		{
			base.Configure(builder);

			builder.ToTable("function");

			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
			builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
			builder.Property(x => x.Enabled).HasColumnName("enabled");
			builder.Property(x => x.Expired).HasColumnName("expired");
			builder.Property(x => x.ModificationTime).UseUnixTime();
			builder.Property(x => x.CreationTime).UseUnixTime();
			
			builder.HasIndex(x => x.Name).IsUnique();
		}
	}
}