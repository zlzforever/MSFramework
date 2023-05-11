using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.AspNetCore.Test.DataModel;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.EntityConfiguration;

public class TestDataConfiguration : EntityTypeConfigurationBase<TestData, TestDataContext>
{
    public override void Configure(EntityTypeBuilder<TestData> builder)
    {
    }
}
