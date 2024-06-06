using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace MicroserviceFramework.Auditing.Loki;

public class LokiAuditingStore : IAuditingStore
{
    private static readonly MessageTemplate Template;
    private static readonly MessageTemplate OperationTemplate;

    static LokiAuditingStore()
    {
        var text1 =
            "{IP:l} - {UserId:l} {Url:l} {Type:l} {EntityId:l} {OperationType:l}";
        Template = new MessageTemplateParser().Parse(text1);

        var text2 =
            "{IP:l} - {UserId:l} {Url:l} [{StartTime:l}] {Elapsed}";
        OperationTemplate = new MessageTemplateParser().Parse(text2);
    }

    public Task AddAsync(AuditOperation auditOperation)
    {
        Debug.Assert(auditOperation.CreationTime != null, "auditOperation.CreationTime != null");

        var auditOperationProperties = new List<LogEventProperty>
        {
            new("Url", new ScalarValue(auditOperation.Url)),
            new("IP", new ScalarValue(auditOperation.IP)),
            new("DeviceId", new ScalarValue(auditOperation.DeviceId ?? string.Empty)),
            new("DeviceModel", new ScalarValue(auditOperation.DeviceModel ?? string.Empty)),
            new("Lat",
                new ScalarValue(auditOperation.Lat.HasValue ? auditOperation.Lat.ToString() : string.Empty)),
            new("Lng",
                new ScalarValue(auditOperation.Lng.HasValue ? auditOperation.Lng.ToString() : string.Empty)),
            new("UserAgent", new ScalarValue(auditOperation.UserAgent)),
            new("StartTime", new ScalarValue(auditOperation.CreationTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))),
            new("EndTime", new ScalarValue(auditOperation.EndTime.ToString("yyyy-MM-dd HH:mm:ss"))),
            new("Elapsed", new ScalarValue(auditOperation.Elapsed)),
            // new("TraceId", new ScalarValue(auditOperation.TraceId)),
            // new("UserId", new ScalarValue(auditOperation.CreatorId)),
            new("OperationId", new ScalarValue(auditOperation.Id)),
            new("Classification", new ScalarValue("Auditing"))
        };
        var auditOperationEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, OperationTemplate,
            auditOperationProperties);
        Log.Logger.Write(auditOperationEvent);

        foreach (var auditEntity in auditOperation.Entities)
        {
            var properties = new List<LogEventProperty>
            {
                new("OperationId", new ScalarValue(auditOperation.Id)),
                new("OperationType", new ScalarValue(auditEntity.OperationType.Id)),
                new("EntityId", new ScalarValue(auditEntity.EntityId)),
                new("Type", new ScalarValue(auditEntity.Type)),
                new("Classification", new ScalarValue("Auditing")),
                // new("TraceId", new ScalarValue(auditOperation.TraceId)),
                // new("UserId", new ScalarValue(auditOperation.CreatorId)),
            };

            foreach (var property in auditEntity.Properties)
            {
                properties.Add(new LogEventProperty($"{property.Name}_Type", new ScalarValue(property.Type)));
                // properties.Add(new LogEventProperty($"{property.Name}_OriginalValue",
                //     new ScalarValue(property.OriginalValue)));
                properties.Add(new LogEventProperty($"{property.Name}_NewValue", new ScalarValue(property.NewValue)));
            }

            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, Template, properties);
            Log.Logger.Write(logEvent);
        }

        return Task.CompletedTask;
    }

    // public Task CommitAsync()
    // {
    //     return Task.CompletedTask;
    // }
}
