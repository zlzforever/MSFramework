using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure;

public class TestDataContext(
    DbContextOptions options,
    IMediator domainEventDispatcher,
    ISession session)
    : DbContextBase(options, domainEventDispatcher, session);
