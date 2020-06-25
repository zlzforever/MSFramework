using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Audit
{
	public class DefaultAuditService : IAuditService
	{
		private readonly IRepository<AuditedOperation> _repository;
		private readonly ILogger<DefaultAuditService> _logger;

		public DefaultAuditService(IServiceProvider serviceProvider,
			ILogger<DefaultAuditService> logger)
		{
			_logger = logger;
			_repository = serviceProvider.GetService<IRepository<AuditedOperation>>();
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

		public bool Enabled => _repository != null;
	}
}