using MicroserviceFramework.Extensions.Options;

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
