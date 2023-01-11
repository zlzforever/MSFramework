using Microsoft.AspNetCore.Builder;

namespace DotNetCore.CAP.Dapr;

public static class DaprExtensions
{
    public static WebApplication Web;

    public static void UseDaprCap(this WebApplication app)
    {
        Web = app;
    }
}
