using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Infrastructure.EntityConfigurations.Audits
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditProperty, TemplateDbContext>
	{
		public override void Configure(EntityTypeBuilder<AuditProperty> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_property");
			
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.Name).HasMaxLength(255);
			builder.Property(x => x.Type).HasMaxLength(255);
			builder.Property(x => x.NewValue);
			builder.Property(x => x.OriginalValue);
		}
	}
}