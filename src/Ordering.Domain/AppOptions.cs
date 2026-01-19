using MicroserviceFramework.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Ordering.Domain;

[AutoOptions]
public class AppOptions
{
    public string ApiName { get; set; }
}

[AutoOptions(Section = "Email")]
public class EmailOptions
{
    public string Address { get; set; }
}
