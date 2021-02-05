using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.FeatureManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.EntityConfigurations.FeatureManagement
{
	public class FeatureTypeConfiguration : EntityTypeConfigurationBase<Feature, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<Feature> builder)
		{
			base.Configure(builder);

			builder.ToTable("feature");

			builder.Property(x => x.Id);
			builder.Property(x => x.Name).HasMaxLength(255);
			builder.Property(x => x.Description).HasMaxLength(1024);
			builder.Property(x => x.Enabled);
			builder.Property(x => x.Expired);
			builder.Property(x => x.ModificationTime).UseUnixTime();
			builder.Property(x => x.CreationTime).UseUnixTime();
			
			builder.HasIndex(x => x.Name).IsUnique();
		}
	}
}