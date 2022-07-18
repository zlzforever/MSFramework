using MicroserviceFramework;
using MicroserviceFramework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MSFramework.Tests;

public class ApplicationInfoTests
{
    [Fact]
    public void GetApplicationName()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(collection =>
            {
                collection.AddMicroserviceFramework();
            }).Build();
        var application = host.Services.GetRequiredService<ApplicationInfo>();
        Assert.Equal("ReSharperTestRunner", application.Name);
    }
}
