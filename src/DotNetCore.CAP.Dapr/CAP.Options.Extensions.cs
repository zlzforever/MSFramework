using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Dapr;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// CAP 选项 Dapr 集成扩展方法
/// </summary>
public static class CapOptionsExtensions
{
    /// <summary>
    /// 配置 CAP 使用 Dapr 作为消息传输层
    /// </summary>
    /// <param name="options">CAP 选项</param>
    /// <param name="configure">Dapr 选项配置委托</param>
    /// <returns>CAP 选项</returns>
    /// <exception cref="ArgumentNullException">configure 为空</exception>
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
