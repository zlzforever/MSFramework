using System;

namespace MSFramework.Domain.Event
{
    public interface IHandlerFactory
    {
        object Create(Type handlerType);
    }
}