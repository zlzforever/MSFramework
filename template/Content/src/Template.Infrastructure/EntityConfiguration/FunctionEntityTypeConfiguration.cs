using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Function;

namespace Template.Infrastructure.EntityConfiguration
{
	public class FunctionEntityTypeConfiguration : EntityTypeConfigurationBase<Function>
	{
		public override Type DbContextType => typeof(AppDbContext);
		
		public override void Configure(EntityTypeBuilder<Function> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Path).IsUnique();
			builder.HasIndex(x => x.Name);
		}
	}
}