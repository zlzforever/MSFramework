using MicroserviceFramework.Domain;

namespace MSFramework.AspNetCore.Test;

public class State(string id, string name) : Enumeration(id, name)
{
    public static readonly State Ok = new State(nameof(Ok), nameof(Ok));
    public static readonly State Error = new State(nameof(Error), nameof(Error));
}
