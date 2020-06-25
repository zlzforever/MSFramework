using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Ef.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServer<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext : DbContextBase
		{
			builder.Services.AddDbContextPool<TDbContext>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;

				var store = EntityFrameworkOptionsStore.LoadFrom(configuration);
				var option = store.Get(dbContextType);
				if (option.DbContextType != dbContextType)
				{
					throw new ArgumentException("DbContextType is not correct");
				}

				if (option.EnableSensitiveDataLogging)
				{
					x.EnableSensitiveDataLogging();
				}

				x.UseSqlServer(option.ConnectionString, options => { options.MigrationsAssembly(entryAssemblyName); });
			});
			return builder;
		}
	}
}