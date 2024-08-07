using MicroserviceFramework.Application;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 默认 EntityFramework 数据上下文
/// </summary>
public class DefaultDbContext(
    DbContextOptions options,
    IMediator domainEventDispatcher,
    ISession session)
    : DbContextBase(options, domainEventDispatcher, session);
