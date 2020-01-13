using System.Threading.Tasks;
using MSFramework.Audit;
using MSFramework.Domain.Repository;

namespace MSFramework.Ef.Audit
{
	public class EfAuditStore : IAuditStore
	{
		private readonly IRepository<AuditOperation> _repository;

		public EfAuditStore(EfRepository<AuditOperation> efRepository)
		{
			_repository = efRepository;
		}

		public async Task SaveAsync(AuditOperation operationEntry)
		{
		await	_repository.InsertAsync(operationEntry);
		}
	}
}