using DotNetCore.CAP;
using DotNetCore.CAP.Dapr;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CapOptionsExtensions
{
    public static CapOptions UseDapr(this CapOptions options, Action<DaprOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        options.RegisterExtension(new DaprCapOptionsExtension(configure));

        return options;
    }
}
