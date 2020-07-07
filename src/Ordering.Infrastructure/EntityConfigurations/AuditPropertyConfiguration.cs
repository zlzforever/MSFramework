using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditProperty, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditProperty> builder)
		{
			base.Configure(builder);

			builder.Property(x => x.Name).HasMaxLength(256);
			builder.Property(x => x.Type).HasMaxLength(256);
		}
	}
}