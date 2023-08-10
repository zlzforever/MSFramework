// using Microsoft.Extensions.Configuration;
//
// namespace MicroserviceFramework.Auditing;
//
// public static class ConfigurationExtensions
// {
//     public static LokiOptions GetLokiUrlFromSerilog(this IConfiguration configuration)
//     {
//         var section = configuration.GetSection("Serilog:WriteTo");
//
//         foreach (var child in section.GetChildren())
//         {
//             var name = child["Name"];
//             if (!string.IsNullOrEmpty(name) && name.Contains("Loki"))
//             {
//                 return new LokiOptions { Url = child["Args:uri"] };
//             }
//         }
//
//         return new LokiOptions();
//     }
// }
