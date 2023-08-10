using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace MicroserviceFramework.Auditing.Loki;

public class LokiAuditingStore : IAuditingStore
{
    private static readonly MessageTemplate Template;

    static LokiAuditingStore()
    {
        var text =
            "{Url:l} {IP:l} {Type:l} {EntityId:l} {OperationType:l} {EndTime:l} {Elapsed} {DeviceModel:l} {Lat:l} {Lng:l}";
        Template = new MessageTemplateParser().Parse(text);
    }

    public Task AddAsync(object sender, AuditOperation auditOperation)
    {
        foreach (var auditEntity in auditOperation.Entities)
        {
            var properties = new List<LogEventProperty>
            {
                new("DeviceId", new ScalarValue(auditOperation.DeviceId ?? string.Empty)),
                new("DeviceModel", new ScalarValue(auditOperation.DeviceModel ?? string.Empty)),
                new("Elapsed", new ScalarValue(auditOperation.Elapsed)),
                new("EndTime", new ScalarValue(auditOperation.EndTime.ToString("yyyy-MM-dd HH:mm:ss"))),
                new("EntityId", new ScalarValue(auditEntity.EntityId)),
                new("IP", new ScalarValue(auditOperation.IP)),
                new("Lat",
                    new ScalarValue(auditOperation.Lat.HasValue ? auditOperation.Lat.ToString() : string.Empty)),
                new("Lng",
                    new ScalarValue(auditOperation.Lng.HasValue ? auditOperation.Lng.ToString() : string.Empty)),
                new("OperationType", new ScalarValue(auditEntity.OperationType)),
                new("Type", new ScalarValue(auditEntity.Type)),
                new("Url", new ScalarValue(auditOperation.Url)),
                new("Classification", new ScalarValue("Auditing")),
                new("SourceContext", new ScalarValue("Auditing"))
            };
            foreach (var property in auditEntity.Properties)
            {
                properties.Add(new LogEventProperty($"{property.Name}Type", new ScalarValue(property.Type)));
                properties.Add(new LogEventProperty($"{property.Name}OriginalValue",
                    new ScalarValue(property.OriginalValue)));
                properties.Add(new LogEventProperty($"{property.Name}NewValue", new ScalarValue(property.NewValue)));
            }

            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, Template, properties);
            Log.Logger.Write(logEvent);
        }

        return Task.CompletedTask;
    }
}
