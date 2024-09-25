using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Dapr;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class CapOptionsExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
