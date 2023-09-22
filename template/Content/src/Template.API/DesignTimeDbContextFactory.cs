using System;
using MicroserviceFramework.Ef.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Infrastructure;

namespace Template.API
{
    public class DesignTimeDbContextFactory
        : DesignTimeDbContextFactoryBase<TemplateDbContext>
    {
        protected override IServiceProvider GetServiceProvider()
        {
            return Program.CreateWebApplicationBuilder(Array.Empty<string>()).Build().Services;
        }

        public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(x => { x.AddConsole(); });
            serviceCollection.ClearForeignKeys();
        }
    }
}
