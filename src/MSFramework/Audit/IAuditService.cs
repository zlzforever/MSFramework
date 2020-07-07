using MSFramework.DependencyInjection;

namespace MSFramework.Audit
{
	public interface IAuditService : IScopeDependency
	{
		/// <summary>
		/// 异步保存实体审计数据
		/// </summary>
		/// <param name="auditOperation">操作审计数据</param>
		/// <returns></returns>
		void Save(AuditOperation auditOperation);
	}
}