using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditedEntity, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditedEntity> builder)
		{
			base.Configure(builder);

			builder.HasIndex(m => m.EntityId);
		}
	}
}