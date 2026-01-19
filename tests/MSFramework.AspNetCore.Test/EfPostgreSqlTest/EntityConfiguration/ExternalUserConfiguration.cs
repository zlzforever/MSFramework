using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.AspNetCore.Test.DataModel;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.EntityConfiguration;

public class ExternalUserConfiguration
    : EntityTypeConfigurationBase<ExternalUser, TestDataContext>
{
    public override void Configure(EntityTypeBuilder<ExternalUser> builder)
    {
        ConfigureDefaultIdentifier(builder);
    }
}
