using Microsoft.Extensions.Logging;

namespace MSFramework.Audits
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

		public void Save(AuditOperation auditOperation)
		{
			if (_repository == null)
			{
				_logger.LogWarning("Audit repository is missing");
				return;
			}

			_repository.Insert(auditOperation);
		}
	}
}