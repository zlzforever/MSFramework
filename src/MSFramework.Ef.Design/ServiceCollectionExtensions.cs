using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef.Design
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ClearForeignKeys(this IServiceCollection serviceCollection)
		{
			serviceCollection.RemoveAll<ICSharpMigrationOperationGenerator>();
			serviceCollection
				.AddSingleton<ICSharpMigrationOperationGenerator, ClearForeignKeysCSharpMigrationOperationGenerator>();
			return serviceCollection;
		}
	}
}