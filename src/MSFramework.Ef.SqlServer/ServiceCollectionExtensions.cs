using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServer<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext : DbContextBase
		{
			builder.Services.AddDbContext<TDbContext>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;

				var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
				var option = optionDict.Get(dbContextType);

				x.UseSqlServer(option.ConnectionString, options =>
				{
					options.MigrationsHistoryTable(option.TablePrefix + "migrations_history");
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
				});
			});
			return builder;
		}
	}
}