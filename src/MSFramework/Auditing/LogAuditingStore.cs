using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Auditing;

public class LogAuditingStore : IAuditingStore
{
    private readonly ILogger _logger;

    public LogAuditingStore(ILoggerFactory logger)
    {
        _logger = logger.CreateLogger("Auditing");
    }

    public Task AddAsync(AuditOperation auditOperation)
    {
        foreach (var auditEntity in auditOperation.Entities)
        {
            _logger.LogInformation(
                "{Type} {EntityId} {OperationType} {Url} {IP} {EndTime} {Elapsed} {DeviceId} {DeviceModel} {Lat} {Lng} {Changes}"
                , auditEntity.Type, auditEntity.EntityId, auditEntity.OperationType, auditOperation.Url,
                auditOperation.IP, auditOperation.EndTime, auditOperation.Elapsed, auditOperation.DeviceId,
                auditOperation.DeviceModel, auditOperation.Lat, auditOperation.Lng,
                Defaults.JsonHelper.Serialize(auditEntity.Properties));
        }

        return Task.CompletedTask;
    }
}
