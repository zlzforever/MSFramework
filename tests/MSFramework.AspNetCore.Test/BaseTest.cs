using System.Net.Http;
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
        // Arrange
        Server = new TestServer(new WebHostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json");
            } )
            .UseStartup<Startup>());
        Client = Server.CreateClient();
    }
}
