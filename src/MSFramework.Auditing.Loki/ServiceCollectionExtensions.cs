using MicroserviceFramework.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLokiAuditing(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<LokiOptions>>().Value;
            var app = provider.GetRequiredService<ApplicationInfo>();
            return LokiAuditingStore.Create(options, app.Name);
        });
        // builder.Services.AddSingleton(provider =>
        // {
        //     var serilogOptions = provider.GetRequiredService<IOptions<SerilogOptions>>().Value;
        //     var options = serilogOptions.WriteTo.FirstOrDefault(x => x.Name.Contains("Loki"));
        //     if (options == null)
        //     {
        //         throw new MicroserviceFrameworkException("未找到 Serilog 中 Loki 的配置项");
        //     }
        //
        //     var lokiOptions = new LokiOptions
        //     {
        //         Uri = options.Args.Uri, Labels = options.Args.Labels.ToDictionary(x => x.Key, x => x.Value)
        //     };
        //     lokiOptions.Labels.Add("Classification", "Auditing");
        //     return lokiOptions;
        // });
        return services;
    }

    /// <summary>
    /// 使用 Loki 存储审计日志
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseLokiAuditing(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddLokiAuditing();
        return builder;
    }
}
