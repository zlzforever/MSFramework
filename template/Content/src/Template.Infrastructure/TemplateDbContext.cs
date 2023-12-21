using MicroserviceFramework.Application;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Template.Infrastructure;

public class TemplateDbContext(
    DbContextOptions options,
    IOptions<DbContextSettingsList> entityFrameworkOptions,
    IMediator mediator,
    ISession session)
    : DbContextBase(options, entityFrameworkOptions, mediator, session);
