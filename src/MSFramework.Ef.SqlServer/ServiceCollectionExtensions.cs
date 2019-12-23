using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Ef.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServerDbContextOptionsBuilderCreator(
			this EntityFrameworkBuilder builder)
		{
			builder.Services.AddSqlServerDbContextOptionsBuilderCreator();
			return builder;
		}

		public static IServiceCollection AddSqlServerDbContextOptionsBuilderCreator(
			this IServiceCollection service)
		{
			service.AddSingleton<IDbContextOptionsBuilderCreator, DbContextOptionsBuilderCreator>();
			return service;
		}
	}
}