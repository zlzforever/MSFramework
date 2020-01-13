using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditOperation>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		/// <summary>
		/// 重写以实现实体类型各个属性的数据库配置
		/// </summary>
		/// <param name="builder">实体类型创建器</param>
		public override void Configure(EntityTypeBuilder<AuditOperation> builder)
		{
		}
	}
}