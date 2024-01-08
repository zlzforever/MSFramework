using System;

namespace MicroserviceFramework.LocalEvent;

internal static class Defaults
{
    public static readonly Type EventHandlerType = typeof(IEventHandler<>);
}
