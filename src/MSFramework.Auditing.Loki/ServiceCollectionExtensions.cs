using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Auditing.Loki;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLokiAuditing(this IServiceCollection services)
    {
        services.AddSingleton<IAuditingStore, LokiAuditingStore>();
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
