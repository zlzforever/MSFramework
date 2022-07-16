using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Audit
{
    /// <summary>
    /// IAuditService 不能和 HttpContext 为同一个 Scope，不然此处的 UOW save 会导致业务数据保存
    /// </summary>
    public class LoggerAuditStore : IAuditStore
    {
        private readonly ILogger<LoggerAuditStore> _logger;

        public LoggerAuditStore(
            ILogger<LoggerAuditStore> logger)
        {
            _logger = logger;
        }

        public Task AddAsync(AuditOperation auditOperation)
        {
            _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(auditOperation));
            return Task.CompletedTask;
        }

        public Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}