using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure;

public class TemplateDbContext(
    DbContextOptions options,
    IMediator mediator,
    ISession session)
    : DbContextBase(options, mediator, session);
