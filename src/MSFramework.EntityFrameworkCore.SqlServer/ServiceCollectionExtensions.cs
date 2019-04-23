using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EntityFrameworkCore.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServerDbContextOptionsBuilderCreator(
			this EntityFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IDbContextOptionsBuilderCreator, DbContextOptionsBuilderCreator>();
			return builder;
		}
	}
}