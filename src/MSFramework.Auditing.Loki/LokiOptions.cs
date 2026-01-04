using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MicroserviceFramework.Extensions.Options;
using Serilog.Sinks.Grafana.Loki;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///
/// </summary>
[AutoOptions(Section = "LokiAuditing")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class LokiOptions
{
    /// <summary>
    ///
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    ///
    /// </summary>
    public List<string> PropertiesAsLabels { get; set; }

    /// <summary>
    ///
    /// </summary>
    public LokiCredentials Credentials { get; set; }
}
