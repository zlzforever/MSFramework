using MicroserviceFramework.Ef;
using MSFramework.AspNetCore.Test.DataModel;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.EntityConfiguration
{
    public class TestDataConfiguration : EntityTypeConfigurationBase<TestData, TestDataContext>
    {
    }
}