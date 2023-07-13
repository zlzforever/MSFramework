using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Commands;

public class TestCommand1Handler : IRequestHandler<TestCommand1, string>
{
    private readonly ILogger<TestCommand1Handler> _logger;

    public TestCommand1Handler(ILogger<TestCommand1Handler> logger)
    {
        _logger = logger;
    }

    public Task<string> HandleAsync(TestCommand1 command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Execute command1: {Value}", command.Value);
        return Task.FromResult(command.Value + ": " + new Random().Next(1, 1000));
    }
}
