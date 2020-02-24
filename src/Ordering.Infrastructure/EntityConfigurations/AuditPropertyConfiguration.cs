using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditProperty>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		/// <summary>
		/// 重写以实现实体类型各个属性的数据库配置
		/// </summary>
		/// <param name="builder">实体类型创建器</param>
		public override void Configure(EntityTypeBuilder<AuditProperty> builder)
		{
			base.Configure(builder);
			
			builder.HasIndex(m => m.AuditEntityId);
			builder.HasOne(m => m.AuditEntity)
				.WithMany(n => n.Properties)
				.HasForeignKey(m => m.AuditEntityId);
		}
	}
}