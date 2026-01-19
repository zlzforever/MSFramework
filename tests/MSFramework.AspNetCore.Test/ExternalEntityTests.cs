using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Test.DataModel;
using Xunit;

namespace MSFramework.AspNetCore.Test;

public class ExternalEntityTests : BaseTest
{
    [Fact]
    public void GetRepo()
    {
        Server.Services.GetRequiredService<IExternalEntityRepository<ExternalUser, string>>();
    }
}
