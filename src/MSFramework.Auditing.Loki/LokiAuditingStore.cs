using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Grafana.Loki;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///
/// </summary>
public class LokiAuditingStore : IAuditingStore
{
    private readonly ILogger _logger;
    private readonly MessageTemplate _template;
    private readonly MessageTemplate _operationTemplate;

    private LokiAuditingStore(ILogger logger, MessageTemplate template, MessageTemplate operationTemplate)
    {
        this._logger = logger;
        this._template = template;
        this._operationTemplate = operationTemplate;
    }

    internal static IAuditingStore Create(LokiOptions options, string application)
    {
        if (string.IsNullOrEmpty(options.Uri))
        {
            throw new ArgumentException("Loki Uri 不能为空");
        }

        var text1 =
            "{IP:l} - {UserId:l} {Url:l} {Type:l} {EntityId:l} {OperationType:l}";
        var template = new MessageTemplateParser().Parse(text1);
        var text2 =
            "{IP:l} - {UserId:l} {Url:l} [{StartTime:l}] {Elapsed}";
        var operationTemplate = new MessageTemplateParser().Parse(text2);

        var directory = "auditing-log";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.GrafanaLoki(options.Uri,
                new List<LokiLabel> { new() { Key = "application", Value = $"{application}-auditing" } },
                options.PropertiesAsLabels, options.Credentials)
            .WriteTo.Async(x => x.File($"{directory}/log.txt", rollingInterval: RollingInterval.Day))
            .CreateLogger();
        var store = new LokiAuditingStore(logger, template, operationTemplate);

        return store;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="auditOperation"></param>
    /// <returns></returns>
    public Task AddAsync(AuditOperation auditOperation)
    {
        if (_logger == null)
        {
            return Task.CompletedTask;
        }

        Debug.Assert(auditOperation.CreationTime != null, "auditOperation.CreationTime != null");

        var auditOperationProperties = new List<LogEventProperty>
        {
            new("Url", new ScalarValue(auditOperation.Path ?? string.Empty)),
            new("IP", new ScalarValue(auditOperation.IP ?? string.Empty)),
            new("DeviceId", new ScalarValue(auditOperation.DeviceId ?? string.Empty)),
            new("DeviceModel", new ScalarValue(auditOperation.DeviceModel ?? string.Empty)),
            new("Lat",
                new ScalarValue(auditOperation.Lat.HasValue ? auditOperation.Lat.ToString() : string.Empty)),
            new("Lng",
                new ScalarValue(auditOperation.Lng.HasValue ? auditOperation.Lng.ToString() : string.Empty)),
            new("UserAgent", new ScalarValue(auditOperation.UserAgent ?? string.Empty)),
            new("StartTime", new ScalarValue(auditOperation.CreationTime.Value.ToString("yyyy-MM-dd HH:mm:ss"))),
            new("EndTime", new ScalarValue(auditOperation.EndTime.ToString("yyyy-MM-dd HH:mm:ss"))),
            new("Elapsed", new ScalarValue(auditOperation.Elapsed)),
            new("TraceId", new ScalarValue(auditOperation.TraceId ?? string.Empty)),
            new("UserId", new ScalarValue(auditOperation.CreatorId ?? string.Empty)),
            new("UserName", new ScalarValue(auditOperation.CreatorName ?? string.Empty)),
            new("OperationId", new ScalarValue(auditOperation.Id))
        };
        var auditOperationEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, _operationTemplate,
            auditOperationProperties);
        _logger.Write(auditOperationEvent);

        foreach (var auditEntity in auditOperation.Entities)
        {
            var properties = new List<LogEventProperty>
            {
                new("OperationId", new ScalarValue(auditOperation.Id)),
                new("OperationType", new ScalarValue(auditEntity.OperationType.Id)),
                new("EntityId", new ScalarValue(auditEntity.EntityId)),
                new("Type", new ScalarValue(auditEntity.Type))
            };

            foreach (var property in auditEntity.Properties)
            {
                properties.Add(new LogEventProperty($"{property.Name}_Type", new ScalarValue(property.Type)));
                properties.Add(new LogEventProperty($"{property.Name}_OriginalValue",
                    new ScalarValue(property.OriginalValue)));
                properties.Add(new LogEventProperty($"{property.Name}_NewValue", new ScalarValue(property.NewValue)));
            }

            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, _template, properties);
            _logger.Write(logEvent);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 这方法，不需要做提交，Serilog 有缓存、批次管理
    /// </summary>
    /// <returns></returns>
    public Task CommitAsync()
    {
        return Task.CompletedTask;
    }
}
