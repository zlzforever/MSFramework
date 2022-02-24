using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Audit
{
	public interface IAuditStore : IDisposable
	{
		/// <summary>
		/// 异步保存实体审计数据
		/// </summary>
		/// <param name="auditOperation">操作审计数据</param>
		/// <returns></returns>
		Task AddAsync(AuditOperation auditOperation);

		Task FlushAsync();
	}
}