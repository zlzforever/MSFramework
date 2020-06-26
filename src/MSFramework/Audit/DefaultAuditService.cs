using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Audit
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

		public void Save(AuditedOperation auditOperation)
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