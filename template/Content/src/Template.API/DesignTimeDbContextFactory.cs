using System;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using Template.Infrastructure;

namespace Template.API
{
	public class DesignTimeDbContextFactory
		: DesignTimeDbContextFactoryBase<AppDbContext>
	{
		protected override void Configure(IServiceCollection services)
		{
			services.AddMySql<AppDbContext>(false);
		}
	}
}