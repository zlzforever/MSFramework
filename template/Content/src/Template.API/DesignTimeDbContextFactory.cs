using System;
using MSFramework.Ef.Design;
using Template.Infrastructure;

namespace Template.API
{
	public class DesignTimeDbContextFactory
		: DesignTimeDbContextFactoryBase<AppDbContext>
	{
		protected override IServiceProvider GetServiceProvider()
		{
			return Program.CreateHostBuilder(new string[0]).Build().Services;
		}
	}
}