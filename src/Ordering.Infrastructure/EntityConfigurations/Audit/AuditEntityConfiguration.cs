using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.EntityConfigurations.Audit
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditEntity, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditEntity> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_entity");

			builder.HasMany(x => x.Properties).WithOne().HasForeignKey("audit_entity_id");

			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(255);
			builder.Property(x => x.Type).HasColumnName("type_name").HasMaxLength(255);
			builder.Property(e => e.OperationType).HasColumnName("operation_type").HasMaxLength(255).UseEnumeration();

			builder.HasIndex(m => m.EntityId);
		}
	}
}