using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetCore.CAP.Dapr;

/// <summary>
///
/// </summary>
[StructLayout(LayoutKind.Auto)]
public record DaprTransportMessage
{
    /// <summary>
    /// Gets the headers of this message
    /// </summary>
    public IDictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Gets the body object of this message
    /// </summary>
    public string Body { get; set; }
}
