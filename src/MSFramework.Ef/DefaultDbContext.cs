using MicroserviceFramework.Application;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef;

/// <summary>
/// 默认EntityFramework数据上下文
/// </summary>
public class DefaultDbContext : DbContextBase
{
    public DefaultDbContext(DbContextOptions options,
        IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
        IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
        options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
    {
    }
}
