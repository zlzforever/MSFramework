// using Microsoft.Extensions.DependencyInjection;
//
// namespace MicroserviceFramework.Auditing;
//
// public static class ServiceCollectionExtensions
// {
//     /// <summary>
//     /// 审计领域的模型（Type）在同个应用里面（即便是多 DbContext）也应该只配置在一个 DbContext 里面；
//     /// 或者审计领域独享一个 DbContext 
//     /// </summary>
//     /// <param name="builder"></param>
//     /// <typeparam name="TAuditStore"></typeparam>
//     /// <returns></returns>
//     public static MicroserviceFrameworkBuilder UseAuditStore<TAuditStore>(this MicroserviceFrameworkBuilder builder)
//         where TAuditStore : class, IAuditStore
//     {
//         builder.Services.AddScoped<IAuditStore, TAuditStore>();
//         return builder;
//     }
//
//     public static MicroserviceFrameworkBuilder UseAuditStore(this MicroserviceFrameworkBuilder builder)
//     {
//         builder.UseAuditStore<LoggerAuditStore>();
//         return builder;
//     }
// }
