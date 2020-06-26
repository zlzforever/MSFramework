using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditedOperation, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<AuditedOperation> builder)
		{
			base.Configure(builder);

			builder.Property(x => x.Ip).HasMaxLength(256);
			builder.Property(x => x.Path).HasMaxLength(256);
			builder.Property(x => x.ApplicationName).HasMaxLength(256);
			builder.Property(x => x.UserAgent).HasMaxLength(256);
			builder.Property(x => x.CreationUserId).HasMaxLength(256);
			builder.Property(x => x.CreationUserName).HasMaxLength(256);
		}
	}
}