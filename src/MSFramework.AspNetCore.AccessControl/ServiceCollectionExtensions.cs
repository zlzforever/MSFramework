using Cerberus.AspNetCore.AccessControl;
using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.AspNetCore.AccessControl;

public static class ServiceCollectionExtensions
{
    public static MicroserviceFrameworkBuilder UseAccessControl(this MicroserviceFrameworkBuilder builder,
        IConfiguration configuration)
    {
        builder.Services.AddAccessControl(configuration);
        return builder;
    }
}
