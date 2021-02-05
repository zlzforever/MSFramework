using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.EntityConfigurations.Audit
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditProperty, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditProperty> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_property");
			
			builder.Property(x => x.Id);
			builder.Property(x => x.Name).HasMaxLength(255);
			builder.Property(x => x.Type).HasMaxLength(255);
			builder.Property(x => x.NewValue);
			builder.Property(x => x.OriginalValue);
		}
	}
}