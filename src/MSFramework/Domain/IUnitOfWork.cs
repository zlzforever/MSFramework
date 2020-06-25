using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Audit;

namespace MSFramework.Domain
{
	public interface IUnitOfWork : IDisposable
	{
		IEnumerable<AuditedEntity> GetAuditEntities();

		void Commit();

		Task CommitAsync();
	}
}