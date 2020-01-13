using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Template.Infrastructure.EntityConfiguration
{
	public class AuditEntityConfiguration : EntityTypeConfigurationBase<AuditEntity>
	{
		public override Type DbContextType => typeof(AppDbContext);
		
		public override void Configure(EntityTypeBuilder<AuditEntity> builder)
		{
			base.Configure(builder);
			
			builder.HasIndex(m => m.OperationId);
			builder.HasIndex(m => m.EntityKey);
			builder.HasOne(m => m.Operation)
				.WithMany(n => n.Entities)
				.HasForeignKey(m => m.OperationId);
		}
	}
}