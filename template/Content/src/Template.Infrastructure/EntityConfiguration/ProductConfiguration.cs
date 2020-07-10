using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Ef.Extensions;
using Template.Domain.AggregateRoot;

namespace Template.Infrastructure.EntityConfiguration
{
	public class ProductConfiguration : EntityTypeConfigurationBase<Product,AppDbContext>
	{
		public override void Configure(EntityTypeBuilder<Product> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Name);
			builder.Property(e => e.ProductType).UseEnumeration();
		}
	}
}