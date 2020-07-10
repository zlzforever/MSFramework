using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using MSFramework.Function;

namespace Template.Infrastructure.EntityConfiguration.Audit
{
    public class FunctionConfiguration : EntityTypeConfigurationBase<FunctionDefine, AppDbContext>
    {
        public override void Configure(EntityTypeBuilder<FunctionDefine> builder)
        {
            base.Configure(builder);

            builder.ToTable("function");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(255);
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
            builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
            builder.Property(x => x.Enabled).HasColumnName("enabled");
            builder.Property(x => x.Expired).HasColumnName("expired");
            builder.ConfigureCreationAudited();
            builder.ConfigureModificationAudited();

            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasIndex(x => x.Name);
        }
    }
}