using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EntityFrameworkCore.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddMySqlDbContextOptionsBuilderCreator(
			this EntityFrameworkBuilder builder)
		{
			builder.Services.AddMySqlDbContextOptionsBuilderCreator();
			return builder;
		}

		public static IServiceCollection AddMySqlDbContextOptionsBuilderCreator(
			this IServiceCollection service)
		{
			service.AddSingleton<IDbContextOptionsBuilderCreator, DbContextOptionsBuilderCreator>();
			return service;
		}
	}
}