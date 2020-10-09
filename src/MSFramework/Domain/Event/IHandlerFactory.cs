using System;

namespace MicroserviceFramework.Domain.Event
{
    public interface IHandlerFactory
    {
        object Create(Type handlerType);
    }
}