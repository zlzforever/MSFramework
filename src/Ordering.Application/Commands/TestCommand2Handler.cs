using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Commands;

public class TestCommand2Handler
    : IRequestHandler<TestCommand2>
{
    private readonly ILogger<TestCommand2Handler> _logger;

    public TestCommand2Handler(ILogger<TestCommand2Handler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TestCommand2 command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Execute command2: {Value}", command.Value);
        return Task.CompletedTask;
    }
}
