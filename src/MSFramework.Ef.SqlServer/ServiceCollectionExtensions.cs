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
			builder.Services.AddDbContext<TDbContext>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;

				var store = new EntityFrameworkOptionsConfiguration(configuration);
				var option = store.Get(dbContextType);

				x.UseSqlServer(option.ConnectionString, options => { options.MigrationsAssembly(entryAssemblyName); });
			});
			return builder;
		}
	}
}