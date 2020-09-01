using System;
using MicroserviceFramework.Ef.Design;
using Ordering.Infrastructure;

namespace Ordering.API
{
	public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<OrderingContext>
	{
		protected override IServiceProvider GetServiceProvider()
		{
			return Program.CreateHostBuilder(new string[0]).Build().Services;
		}
	}
}