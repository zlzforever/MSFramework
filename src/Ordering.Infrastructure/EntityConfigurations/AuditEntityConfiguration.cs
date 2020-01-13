using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;
using MSFramework.Function;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditEntity>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		public override void Configure(EntityTypeBuilder<AuditEntity> builder)
		{
			builder.HasIndex(m => m.OperationId);
			builder.HasOne(m => m.Operation)
				.WithMany(n => n.Entities)
				.HasForeignKey(m => m.OperationId);
		}
	}
}