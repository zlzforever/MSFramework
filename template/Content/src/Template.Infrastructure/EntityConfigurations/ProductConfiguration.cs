using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Aggregates.Project;

namespace Template.Infrastructure.EntityConfigurations
{
	public class ProductConfiguration : EntityTypeConfigurationBase<Product,TemplateDbContext>
	{
		public override void Configure(EntityTypeBuilder<Product> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Name);
			builder.Property(e => e.ProductType).UseEnumeration();
		}
	}
}