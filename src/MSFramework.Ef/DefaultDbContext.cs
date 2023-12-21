using MicroserviceFramework.Application;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 默认 EntityFramework 数据上下文
/// </summary>
public class DefaultDbContext(
    DbContextOptions options,
    IOptions<DbContextSettingsList> entityFrameworkOptions,
    IMediator domainEventDispatcher,
    ISession session)
    : DbContextBase(options, entityFrameworkOptions, domainEventDispatcher, session);
