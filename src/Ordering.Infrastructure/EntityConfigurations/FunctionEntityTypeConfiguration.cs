using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Function;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class FunctionEntityTypeConfiguration : EntityTypeConfigurationBase<Function>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		public override void Configure(EntityTypeBuilder<Function> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Path).IsUnique();
			builder.HasIndex(x => x.Name);
		}
	}
}