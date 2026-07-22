using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetCore.CAP.Dapr;

/// <summary>
/// Dapr 传输消息，包含消息头和消息体
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
