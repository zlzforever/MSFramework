using System.Threading.Tasks;

namespace MSFramework.Audit
{
	public interface IAuditStore
	{
		/// <summary>
		/// 异步保存实体审计数据
		/// </summary>
		/// <param name="operationEntry">操作审计数据</param>
		/// <returns></returns>
		Task SaveAsync(AuditOperation operationEntry);
	}
}