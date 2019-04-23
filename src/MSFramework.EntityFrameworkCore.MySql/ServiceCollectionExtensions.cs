using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EntityFrameworkCore.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddMySqlDbContextOptionsBuilderCreator(
			this EntityFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IDbContextOptionsBuilderCreator, DbContextOptionsBuilderCreator>();
			return builder;
		}
	}
}