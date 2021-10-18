using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Audit
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 审计领域的模型（Type）在同个应用里面（即便是多 DbContext）也应该只配置在一个 DbContext 里面；
		/// 或者审计领域独享一个 DbContext 
		/// </summary>
		/// <param name="builder"></param>
		/// <typeparam name="TAuditService"></typeparam>
		/// <returns></returns>
		public static MicroserviceFrameworkBuilder UseAudit<TAuditService>(this MicroserviceFrameworkBuilder builder)
			where TAuditService : class, IAuditStore
		{
			builder.Services.TryAddScoped<IAuditStore, TAuditService>();
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseAudit(this MicroserviceFrameworkBuilder builder)
		{
			builder.UseAudit<LoggerAuditStore>();
			return builder;
		}
	}
}