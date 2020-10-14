using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Audit
{
	public class DefaultAuditService : IAuditService
	{
		private readonly IAuditRepository _repository;
		private readonly ILogger<DefaultAuditService> _logger;

		public DefaultAuditService(IAuditRepository repository,
			ILogger<DefaultAuditService> logger)
		{
			_logger = logger;
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
		}
	}
}