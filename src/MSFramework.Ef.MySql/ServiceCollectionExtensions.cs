using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace MSFramework.Ef.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddMySql<TDbContext>(
			this EntityFrameworkBuilder builder) where TDbContext : DbContextBase
		{
			builder.Services.AddMySql<TDbContext>();
			return builder;
		}

		public static IServiceCollection AddMySql<TDbContext>(
			this IServiceCollection services) where TDbContext : DbContextBase
		{
			services.AddDbContextPool<TDbContext>(x =>
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
				var dict = EntityFrameworkOptionDict.LoadFrom(configuration);
				var option = dict.Value[dbContextType.Name];
				if (option.DbContextType != dbContextType)
				{
					throw new ArgumentException("DbContextType is not correct");
				}

				// todo: config 
				x.EnableSensitiveDataLogging();
				x.UseMySql(option.ConnectionString, options =>
				{
					options.MigrationsAssembly(entryAssemblyName);
					options.CharSet(CharSet.Utf8Mb4);
				});
			});
			return services;
		}
	}
}