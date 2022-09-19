using System.Linq;
using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure;

public class OrderingContext2 : DbContextBase
{
    public OrderingContext2(DbContextOptions<OrderingContext2> options,
        IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
        IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
        options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly,
            type => type?.BaseType?.GenericTypeArguments.ElementAtOrDefault(1) == typeof(OrderingContext2));
    }
}
