using System;

namespace MicroserviceFramework.EventBus;

internal static class Defaults
{
    public static readonly Type EventHandlerType = typeof(IEventHandler<>);
}
