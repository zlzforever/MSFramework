using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Function;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Infrastructure.EntityConfigurations.Audits
{
	public class FunctionConfiguration : EntityTypeConfigurationBase<FunctionDefine, TemplateDbContext>
	{
		public override void Configure(EntityTypeBuilder<FunctionDefine> builder)
		{
			base.Configure(builder);

			builder.ToTable("function");

			builder.Property(x => x.Id);
			builder.Property(x => x.Code).HasMaxLength(255);
			builder.Property(x => x.Name).HasMaxLength(255);
			builder.Property(x => x.Description).HasMaxLength(2000);
			builder.Property(x => x.Enabled);
			builder.Property(x => x.Expired);
			builder.ConfigureCreation();
			builder.ConfigureModification();

			builder.HasIndex(x => x.Code).IsUnique();
			builder.HasIndex(x => x.Name);
		}
	}
}