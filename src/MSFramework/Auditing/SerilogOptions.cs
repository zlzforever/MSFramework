// using System.Collections.Generic;
// using MicroserviceFramework.Extensions.Options;
//
// namespace MicroserviceFramework.Auditing;
//
// [OptionsType("Serilog")]
// public class SerilogOptions
// {
//     public List<WriteToOptions> WriteTo { get; set; } = new();
//
//     public class WriteToOptions
//     {
//         public string Name { get; set; }
//         public Arg Args { get; set; }
//
//         public class Arg
//         {
//             public string Uri { get; set; }
//             public List<Label> Labels { get; set; }
//         }
//
//         public class Label
//         {
//             public string Key { get; set; }
//             public string Value { get; set; }
//         }
//     }
// }
//
// public class LokiOptions
// {
//     public string Uri { get; set; }
//     public Dictionary<string, string> Labels { get; set; }
// }
