using System;

namespace MSFramework.Domain.Events
{
    public interface IHandlerFactory
    {
        object Create(Type handlerType);
    }
}