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
			where TAuditService : class, IAuditService
		{
			builder.Services.TryAddScoped<IAuditService, TAuditService>();
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseAudit(this MicroserviceFrameworkBuilder builder)
		{
			builder.UseAudit<DefaultAuditService>();
			return builder;
		}

		// public static bool IsAuditEnabled(this IServiceProvider serviceProvider)
		// {
		// 	var auditService = serviceProvider.GetService<IAuditService>();
		// 	if (auditService == null)
		// 	{
		// 		return false;
		// 	}
		//
		// 	return auditService.Enabled;
		// }
	}
}