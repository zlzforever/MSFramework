using System;
using MicroserviceFramework.Ef.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure;

namespace Ordering.API;

public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<OrderingContext>
{
    protected override IServiceProvider GetServiceProvider()
    {
        Console.WriteLine("CreateServiceProvider");
        return Program.CreateWebApplicationBuilder(Array.Empty<string>()).Build().Services;
    }

    public override void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        Console.WriteLine("ConfigureDesignTimeServices");
        serviceCollection.AddLogging(x => { x.AddConsole(); });
        serviceCollection.ClearForeignKeys();
    }
}
