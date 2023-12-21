using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;

namespace Ordering.API;

public class TestInitializer : IInitializerBase
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public int Order => 1;
}
