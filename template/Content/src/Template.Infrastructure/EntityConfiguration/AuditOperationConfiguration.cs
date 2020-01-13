using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Template.Infrastructure.EntityConfiguration
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditOperation>
	{
		public override Type DbContextType => typeof(AppDbContext);

		public override void Configure(EntityTypeBuilder<AuditOperation> builder)
		{
			base.Configure(builder);

			builder.HasIndex(m => m.FunctionPath);
		}
	}
}