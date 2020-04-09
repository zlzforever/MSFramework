using Microsoft.Extensions.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using Ordering.Infrastructure;

namespace Ordering.API
{
	public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<OrderingContext>
	{
		protected override void Configure(IServiceCollection services)
		{
			services.AddMySql<OrderingContext>(false);
		}
	}
}