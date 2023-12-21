using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

public class TestDataContext(
    DbContextOptions options,
    IOptions<DbContextSettingsList> entityFrameworkOptions,
    IMediator domainEventDispatcher,
    ISession session)
    : DbContextBase(options, entityFrameworkOptions, domainEventDispatcher, session);
