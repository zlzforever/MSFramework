using System;
using MicroserviceFramework.Ef.Design;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Infrastructure;

namespace Ordering.API
{
	public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<OrderingContext>
	{
		protected override IServiceProvider GetServiceProvider()
		{
			return Program.CreateHostBuilder(new string[0]).Build().Services;
		}

		public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
		{
			serviceCollection.ClearForeignKeys();
		}
	}
}