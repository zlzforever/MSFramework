using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure;

public class OrderingContext(
    DbContextOptions<OrderingContext> options,
    IMediator mediator,
    ISession session)
    : DbContextBase(options, mediator, session);
