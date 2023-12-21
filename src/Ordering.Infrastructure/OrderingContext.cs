using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure;

public class OrderingContext(
    DbContextOptions<OrderingContext> options,
    IOptions<DbContextSettingsList> entityFrameworkOptions,
    IMediator domainEventDispatcher,
    ISession session)
    : DbContextBase(options, entityFrameworkOptions, domainEventDispatcher, session);
