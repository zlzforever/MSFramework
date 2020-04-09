using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Ef.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServer<TDbContext>(
			this EntityFrameworkBuilder builder) where TDbContext : DbContextBase
		{
			builder.Services.AddDbContextPool<TDbContext>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;
				var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
				if (string.IsNullOrWhiteSpace(environment))
				{
					environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				}

				var configurationBuilder = new ConfigurationBuilder();
				configurationBuilder.AddJsonFile("appsettings.json");
				if (!string.IsNullOrWhiteSpace(environment))
				{
					configurationBuilder.AddJsonFile($"appsettings.{environment}.json");
				}

				var configuration = configurationBuilder.Build();
				var store = EntityFrameworkOptionsStore.LoadFrom(configuration);
				var option = store.Get(dbContextType);
				if (option.DbContextType != dbContextType)
				{
					throw new ArgumentException("DbContextType is not correct");
				}

				// todo: config 
				x.EnableSensitiveDataLogging();
				x.UseSqlServer(option.ConnectionString, options => { options.MigrationsAssembly(entryAssemblyName); });
			});
			return builder;
		}
	}
}