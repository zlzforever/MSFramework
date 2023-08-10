// comments by lewis at 20230810
// 直接通过 HTTP 发送日志会和业务开销捆绑
// 已经通过 Serilog.Loki 来实现

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Text;
// using System.Threading.Tasks;
// using MicroserviceFramework.Auditing.Model;
// using MicroserviceFramework.Common;
// using Microsoft.Extensions.Options;
//
// namespace MicroserviceFramework.Auditing;
//
// /// <summary>
// ///
// /// </summary>
// public class LokiAuditingStore : IAuditingStore
// {
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly ApplicationInfo _applicationInfo;
//     private readonly LokiOptions _lokiOptions;
//
//     public LokiAuditingStore(IHttpClientFactory httpClientFactory,
//         ApplicationInfo applicationInfo, LokiOptions lokiOptions)
//     {
//         _httpClientFactory = httpClientFactory;
//         _applicationInfo = applicationInfo;
//         _lokiOptions = lokiOptions;
//     }
//
//     public async Task AddAsync(object sender, AuditOperation auditOperation)
//     {
//         var client = _httpClientFactory.CreateClient("Loki");
//         var timestamp = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}000000";
//         var lokiLog = new AuditLog
//         {
//             Streams = new LogItem[]
//             {
//                 new()
//                 {
//                     Stream =
//                         _lokiOptions.Labels,
//                     Values = new List<string[]>()
//                 }
//             }
//         };
//
//         foreach (var auditEntity in auditOperation.Entities)
//         {
//             var dict = new Dictionary<string, string>
//             {
//                 { "DeviceId", auditOperation.DeviceId },
//                 { "DeviceModel", auditOperation.DeviceModel },
//                 { "Elapsed", auditOperation.Elapsed.ToString() },
//                 { "EndTime", auditOperation.EndTime.ToString("yyyy-MM-dd HH:mm:ss") },
//                 { "EntityId", auditEntity.EntityId },
//                 { "IP", auditOperation.IP },
//                 { "Lat", auditOperation.Lat.ToString() },
//                 { "Lng", auditOperation.Lng.ToString() },
//                 { "OperationType", auditEntity.OperationType.Id },
//                 { "Type", auditEntity.Type },
//                 { "Url", auditOperation.Url }
//             };
//             foreach (var property in auditEntity.Properties.OrderBy(x => x.Name))
//             {
//                 dict.Add($"{property.Name}Type", property.Type);
//                 dict.Add($"{property.Name}OriginalValue", property.OriginalValue);
//                 dict.Add($"{property.Name}NewValue", property.NewValue);
//             }
//
//             lokiLog.Streams[0].Values.Add(new[] { timestamp, Defaults.JsonSerializer.Serialize(dict) });
//         }
//
//         for (var i = 0; i <= 3; ++i)
//         {
//             var response = await client.PostAsync($"{_lokiOptions.Uri}/loki/api/v1/push",
//                 new StringContent(Defaults.JsonSerializer.Serialize(lokiLog), Encoding.UTF8, "application/json"));
//             if (response.IsSuccessStatusCode)
//             {
//                 break;
//             }
//
//             await Task.Delay(10);
//         }
//     }
//
//     private class LogItem
//     {
//         public Dictionary<string, string> Stream { get; set; }
//         public List<string[]> Values { get; set; }
//     }
//
//     private class AuditLog
//     {
//         public LogItem[] Streams { get; set; }
//     }
// }
