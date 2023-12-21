using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Commands;

public class TestCommand2Handler(ILogger<TestCommand2Handler> logger) : IRequestHandler<TestCommand2>
{
    public Task HandleAsync(TestCommand2 command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Execute command2: {Value}", command.Value);
        return Task.CompletedTask;
    }
}
