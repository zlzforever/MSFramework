using System.Collections.Generic;
using MicroserviceFramework.Extensions.Options;
using Serilog.Sinks.Grafana.Loki;

namespace MicroserviceFramework.Auditing.Loki;

[OptionsType("LokiAuditing")]
public class LokiOptions
{
    public string Uri { get; set; }
    public List<string> PropertiesAsLabels { get; set; }

    public LokiCredentials Credentials { get; set; }
}
