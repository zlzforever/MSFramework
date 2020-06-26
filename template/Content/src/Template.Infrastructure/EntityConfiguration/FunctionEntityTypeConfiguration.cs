using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Function;
using Template.Infrastructure;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class FunctionEntityTypeConfiguration : EntityTypeConfigurationBase<FunctionDefine, AppDbContext>
	{
		public override void Configure(EntityTypeBuilder<FunctionDefine> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Code).IsUnique();
			builder.HasIndex(x => x.Name);
		}
	}
}