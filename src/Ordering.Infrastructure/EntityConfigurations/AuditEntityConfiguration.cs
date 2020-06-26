using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;
using MSFramework.Ef.Extensions;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditedEntity, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditedEntity> builder)
		{
			base.Configure(builder);

			builder.HasIndex(m => m.EntityId);

			builder.Property(x => x.EntityId).HasMaxLength(256);
			builder.Property(x => x.TypeName).HasMaxLength(256);

			builder.Property(e => e.OperationType).IsEnumeration();
		}
	}
}