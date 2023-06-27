using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ordering.Infrastructure;

public class AuditingContext : DbContextBase
{
    public AuditingContext(DbContextOptions<AuditingContext> options,
        IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
        IMediator domainEventDispatcher, ISession session, ILoggerFactory loggerFactory) : base(
        options, entityFrameworkOptions, domainEventDispatcher, session, loggerFactory)
    {
    }
}
