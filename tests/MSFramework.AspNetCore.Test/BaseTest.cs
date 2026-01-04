using System.IO;
using System.Net.Http;
using MicroserviceFramework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace MSFramework.AspNetCore.Test;

public abstract class BaseTest
{
    protected readonly TestServer Server;
    protected readonly HttpClient Client;

    protected BaseTest()
    {
        if (!Directory.Exists(Defaults.OSSDirectory))
        {
            Directory.CreateDirectory(Defaults.OSSDirectory);
        }

        // Arrange
        Server = new TestServer(new WebHostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json");
            })
            .UseStartup<Startup>());
        Client = Server.CreateClient();
    }
}
