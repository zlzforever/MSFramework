using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;

namespace Ordering.API;

public class TestInitializer : Initializer
{
    public TestInitializer()
    {
        Order = 1;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
