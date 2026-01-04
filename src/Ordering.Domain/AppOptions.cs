using MicroserviceFramework.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Ordering.Domain;

[AutoOptions]
public class AppOptions(IConfiguration configuration)
{
    public string Audience { get; set; }

    public string DefaultConnectionString => configuration["DbContexts:OrderingContext:ConnectionString"];
}

[AutoOptions(Section = "Email")]
public class EmailOptions
{
    public string Address { get; set; }
}
