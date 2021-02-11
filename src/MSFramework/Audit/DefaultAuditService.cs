using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Audit
{
	/// <summary>
	/// IAuditService 不能和 HttpContext 为同一个 Scope，不然此处的 UOW save 会导致业务数据保存
	/// </summary>
	public class DefaultAuditService : IAuditService
	{
		private readonly IAuditRepository _repository;
		private readonly ILogger<DefaultAuditService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public DefaultAuditService(IAuditRepository repository,
			ILogger<DefaultAuditService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
			_repository = repository;
		}

		public async Task AddAsync(AuditOperation auditOperation)
		{
			if (_repository == null)
			{
				_logger.LogWarning("Audit repository is missing");
				return;
			}

			await _repository.AddAsync(auditOperation);
			await _unitOfWork.CommitAsync();
		}
	}
}