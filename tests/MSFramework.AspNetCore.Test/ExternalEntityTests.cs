using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.AspNetCore.Test;

public class ExternalUser(string id) : ExternalEntity<string>(id);

public class ExternalEntityTests : BaseTest
{
    [Fact]
    public void GetRepo()
    {
        Server.Services.GetRequiredService<IExternalEntityRepository<ExternalUser, string>>();
    }
}
