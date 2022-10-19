using MicroserviceFramework.Audit;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Infrastructure.EntityConfigurations.Audits
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditOperation, TemplateDbContext>
	{
		public override void Configure(EntityTypeBuilder<AuditOperation> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_operation");

			builder.HasMany(x => x.Entities).WithOne(x => x.Operation);

			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.IP).HasMaxLength(255);
			builder.Property(x => x.DeviceId).HasMaxLength(36);
			builder.Property(x => x.DeviceModel).HasMaxLength(50);
			builder.Property(x => x.Lat);
			builder.Property(x => x.Lng);
			builder.Property(x => x.UserAgent).HasMaxLength(1024);
			builder.Property(x => x.Url).HasMaxLength(1024);
			builder.Property(x => x.Elapsed);
			builder.Property(x => x.EndTime).UseUnixTime();
			builder.ConfigureCreation();

			builder.HasIndex(x => x.CreatorId);
			builder.HasIndex(x => x.EndTime);
		}
	}
}