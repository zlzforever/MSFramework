using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Audits;

namespace MicroserviceFramework.Domain
{
	public interface IUnitOfWork : IDisposable
	{
		IEnumerable<AuditEntity> GetAuditEntities();
		
		Task<int> CommitAsync();
	}
}