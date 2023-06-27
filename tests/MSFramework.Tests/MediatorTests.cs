using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MSFramework.Tests;

public class Command0 : IRequest
{
    public static int Count;
}

public class CommandHandler0 : IRequestHandler<Command0>
{
    public Task HandleAsync(Command0 command, CancellationToken cancellationToken = default)
    {
        Command0.Count += 1;
        return Task.CompletedTask;
    }
}

public class Command1 : IRequest
{
    public static int Count;
}

public class CommandHandler1 : IRequestHandler<Command1>
{
    public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
    {
        Command1.Count += 1;
        return Task.CompletedTask;
    }
}

public class CommandHandler2 : IRequestHandler<Command1>
{
    public Task HandleAsync(Command1 command, CancellationToken cancellationToken = default)
    {
        Command1.Count += 1;
        return Task.CompletedTask;
    }
}

public class Command2 : IRequest<int>
{
    public static int Count;
}

public class Command2Handler : IRequestHandler<Command2, int>
{
    public Task<int> HandleAsync(Command2 command, CancellationToken cancellationToken = default)
    {
        Command2.Count += 1;
        return Task.FromResult(Command2.Count);
    }
}

public class Command3 : IRequest<int>
{
    public int Count { get; set; } = 1;
}

public class Command3Handler : IRequestHandler<Command3, int>
{
    public Task<int> HandleAsync(Command3 command, CancellationToken cancellationToken = default)
    {
        throw new ArgumentException("test");
    }
}

public class MediatorTests
{
    [Fact]
    public async Task RequestToMultiHandlersTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseMediator();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new Command1());
        Assert.Equal(1, Command1.Count);
        await mediator.SendAsync(new Command1());
        Assert.Equal(2, Command1.Count);
    }

    [Fact]
    public async Task RequestWithoutResponseTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(x => x.AddConsole());
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseMediator();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new Command0());
        Assert.Equal(1, Command0.Count);
        await mediator.SendAsync(new Command0());
        Assert.Equal(2, Command0.Count);
    }

    [Fact]
    public async Task RequestWithResponseTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(x => x.AddConsole());

        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseMediator();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var a = await mediator.SendAsync(new Command2());
        Assert.Equal(1, a);
        a = await mediator.SendAsync(new Command2());
        Assert.Equal(2, a);

        Assert.Equal(2, Command2.Count);
    }

    [Fact]
    public void ThrowExceptionTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseMediator();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        Assert.Throws<ArgumentException>(() =>
        {
            mediator.SendAsync(new Command3()).GetAwaiter().GetResult();
        });
    }
}
