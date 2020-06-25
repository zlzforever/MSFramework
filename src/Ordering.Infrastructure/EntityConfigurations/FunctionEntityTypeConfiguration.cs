using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Function;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class FunctionEntityTypeConfiguration : EntityTypeConfigurationBase<FunctionDefine, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<FunctionDefine> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Code).IsUnique();
			builder.HasIndex(x => x.Name);
		}
	}
}