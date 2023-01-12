using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetCore.CAP.Dapr;

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

    public string GetId()
    {
        return Headers[Messages.Headers.MessageId]!;
    }

    public string GetName()
    {
        return Headers[Messages.Headers.MessageName]!;
    }

    public string GetGroup()
    {
        return Headers.TryGetValue(Messages.Headers.Group, out var value) ? value : null;
    }

    public string GetCorrelationId()
    {
        return Headers.TryGetValue(Messages.Headers.CorrelationId, out var value) ? value : null;
    }
}
