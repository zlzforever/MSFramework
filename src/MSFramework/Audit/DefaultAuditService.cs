using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Audit
{
	public class DefaultAuditService : IAuditService
	{
		private readonly IRepository<AuditOperation> _repository;
		private readonly ILogger<DefaultAuditService> _logger;

		public DefaultAuditService(IServiceProvider serviceProvider, ILogger<DefaultAuditService> logger)
		{
			_logger = logger;
			_repository = serviceProvider.GetService<IRepository<AuditOperation>>();
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