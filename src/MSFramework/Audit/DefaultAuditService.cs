using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Audit
{
	public class DefaultAuditService : IAuditService
	{
		private readonly IAuditRepository _repository;
		private readonly ILogger<DefaultAuditService> _logger;
		private readonly UnitOfWorkManager _unitOfWorkManager;

		public DefaultAuditService(IAuditRepository repository,
			ILogger<DefaultAuditService> logger, UnitOfWorkManager unitOfWorkManager)
		{
			_logger = logger;
			_unitOfWorkManager = unitOfWorkManager;
			_repository = repository;
		}

		public async Task SaveAsync(AuditOperation auditOperation)
		{
			if (_repository == null)
			{
				_logger.LogWarning("Audit repository is missing");
				return;
			}

			await _repository.InsertAsync(auditOperation);
			await _unitOfWorkManager.CommitAsync();
		}
	}
}