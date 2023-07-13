using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace MSFramework.AspNetCore.Test;

public abstract class BaseTest
{
    protected readonly TestServer Server;
    protected readonly HttpClient Client;

    public BaseTest()
    {
        // Arrange
        Server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        Client = Server.CreateClient();
    }
}
