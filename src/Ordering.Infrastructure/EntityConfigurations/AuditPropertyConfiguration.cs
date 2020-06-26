using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditedProperty, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditedProperty> builder)
		{
			base.Configure(builder);

			builder.Property(x => x.PropertyName).HasMaxLength(256);
			builder.Property(x => x.PropertyType).HasMaxLength(256);
		}
	}
}