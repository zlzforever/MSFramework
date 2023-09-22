using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Aggregates.Project;

namespace Template.Infrastructure.EntityConfigurations
{
    public class ProductConfiguration : EntityTypeConfigurationBase<Product, TemplateDbContext>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            ConfigureDefaultIdentifier(builder);

            builder.Property(i => i.Id).HasColumnType("varchar").HasMaxLength(36);
            builder.Property(i => i.Name).HasMaxLength(100);
            builder.Property(e => e.ProductType).UseEnumeration();

            builder.HasIndex(x => x.Name);
            builder.ConfigureCreation();
            builder.ConfigureModification();
        }
    }
}
