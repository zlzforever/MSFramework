using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.AspNetCore.Test.DataModel;
using MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;
using MSFramework.Ef;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.EntityConfiguration
{
	public class TestDataConfiguration: EntityTypeConfigurationBase<TestData, TestDataContext>
	{
	}
}