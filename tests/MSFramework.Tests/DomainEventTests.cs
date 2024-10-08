using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MSFramework.Tests;

public record DomainEvent4 : DomainEvent
{
    public static int Count;
}

public class DomainEvent4Handler : IDomainEventHandler<DomainEvent4>
{
    public Task HandleAsync(DomainEvent4 @event, CancellationToken cancellationToken = default)
    {
        DomainEvent4.Count += 1;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}

public record DomainEvent2 : DomainEvent;

public record DomainEvent3 : DomainEvent
{
    public static int Count;
}

public class DomainEvent31Handler : IDomainEventHandler<DomainEvent3>
{
    public Task HandleAsync(DomainEvent3 @event, CancellationToken cancellationToken = default)
    {
        DomainEvent3.Count += 1;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}

public class DomainEvent32Handler : IDomainEventHandler<DomainEvent3>
{
    public Task HandleAsync(DomainEvent3 @event, CancellationToken cancellationToken = default)
    {
        DomainEvent3.Count += 1;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}

public class DomainEventTests
{
    [Fact]
    public async Task DispatchTo1HandlerTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseAspNetCoreExtension();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.UseMicroserviceFramework();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.PublishAsync(new DomainEvent4());
        Assert.Equal(1, DomainEvent4.Count);

        await mediator.PublishAsync(new DomainEvent4());
        Assert.Equal(2, DomainEvent4.Count);
    }

    [Fact]
    public async Task DispatchToMultiHandlerTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseAspNetCoreExtension();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.UseMicroserviceFramework();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.PublishAsync(new DomainEvent3());
        Assert.Equal(2, DomainEvent3.Count);

        await mediator.PublishAsync(new DomainEvent3());
        Assert.Equal(4, DomainEvent3.Count);
    }

    [Fact]
    public async Task EventWithoutHandlerTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseAspNetCoreExtension();
        });
        serviceCollection.AddLogging(x => x.AddConsole());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.UseMicroserviceFramework();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.PublishAsync(new DomainEvent2());
    }
}
