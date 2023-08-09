using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Common;
using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.Auditing;

public class LokiAuditingStore : IAuditingStore
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ApplicationInfo _applicationInfo;

    public LokiAuditingStore(IHttpClientFactory httpClientFactory, IConfiguration configuration,
        ApplicationInfo applicationInfo)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _applicationInfo = applicationInfo;
    }

    public async Task AddAsync(object sender, AuditOperation auditOperation)
    {
        var loki = _configuration["Loki"];
        if (string.IsNullOrEmpty(loki))
        {
            return;
        }

        var lokiServer = _configuration[$"Serilog:WriteTo:{loki}:Args:uri"];
        if (string.IsNullOrEmpty(lokiServer))
        {
            return;
        }

        var client = _httpClientFactory.CreateClient("Loki");
        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        foreach (var auditEntity in auditOperation.Entities)
        {
            var dict = new Dictionary<string, string>
            {
                { "Category", "Auditing" },
                { "DeviceId", auditOperation.DeviceId },
                { "DeviceModel", auditOperation.DeviceModel },
                { "Elapsed", auditOperation.Elapsed.ToString() },
                { "EndTime", auditOperation.EndTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "EntityId", auditEntity.EntityId },
                { "IP", auditOperation.IP },
                { "Lat", auditOperation.Lat.ToString() },
                { "Lng", auditOperation.Lng.ToString() },
                { "OperationType", auditEntity.OperationType.Id },
                { "Type", auditEntity.Type },
                { "Url", auditOperation.Url }
            };
            foreach (var property in auditEntity.Properties.OrderBy(x => x.Name))
            {
                dict.Add($"{property.Name}Type", property.Type);
                dict.Add($"{property.Name}OriginalValue", property.OriginalValue);
                dict.Add($"{property.Name}NewValue", property.NewValue);
            }

            var lokiLog = new AuditLog
            {
                Streams = new LogItem[]
                {
                    new()
                    {
                        Stream =
                            new Dictionary<string, string> { { "Application", _applicationInfo.Name } },
                        Values = new[] { new[] { $"{timestamp}000000", Defaults.JsonHelper.Serialize(dict) } }
                    }
                }
            };

            for (var i = 0; i <= 3; ++i)
            {
                var response = await client.PostAsync($"{lokiServer}/loki/api/v1/push",
                    new StringContent(Defaults.JsonHelper.Serialize(lokiLog), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    break;
                }

                await Task.Delay(10);
            }
        }
    }

    private class LogItem
    {
        public Dictionary<string, string> Stream { get; set; }
        public string[][] Values { get; set; }
    }

    private class AuditLog
    {
        public LogItem[] Streams { get; set; }
    }
}
