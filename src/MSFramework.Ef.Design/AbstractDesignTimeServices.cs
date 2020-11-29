using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef.Design
{
	public abstract class AbstractDesignTimeServices : IDesignTimeServices
	{
		public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
		{
			serviceCollection.RemoveAll<ICSharpMigrationOperationGenerator>();
			serviceCollection
				.AddSingleton<ICSharpMigrationOperationGenerator, ClearForeignKeysCSharpMigrationOperationGenerator>();
		}
	}
}