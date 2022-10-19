using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Infrastructure.EntityConfigurations.Audits
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditEntity, TemplateDbContext>
	{
		public override void Configure(EntityTypeBuilder<AuditEntity> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_entity");

			builder.HasMany(x => x.Properties).WithOne(x => x.Entity);

			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.EntityId).HasMaxLength(255);
			builder.Property(x => x.Type).HasMaxLength(255);
			builder.Property(e => e.OperationType).HasMaxLength(255).UseEnumeration();

			builder.HasIndex(m => m.EntityId);
		}
	}
}