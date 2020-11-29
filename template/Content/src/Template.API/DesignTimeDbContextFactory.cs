using System;
using MicroserviceFramework.Ef.Design;
using Template.Infrastructure;

namespace Template.API
{
	public class DesignTimeDbContextFactory
		: DesignTimeDbContextFactoryBase<TemplateDbContext>
	{
		protected override IServiceProvider GetServiceProvider()
		{
			return Program.CreateHostBuilder(new string[0]).Build().Services;
		}
	}
}