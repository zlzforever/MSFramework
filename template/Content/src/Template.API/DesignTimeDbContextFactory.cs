using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Ef;
using MSFramework.Ef.MySql;
using MSFramework.Reflection;
using Template.Infrastructure;

namespace Template.API
{
	public class DesignTimeDbContextFactory
		: DesignTimeDbContextFactoryBase<AppDbContext>
	{
		protected override void Configure(IServiceCollection services)
		{
			Console.WriteLine("Configure mysql dbcontext builder creator");
			services.AddMySqlDbContextOptionsBuilderCreator();
		}
	}
}