using System;

namespace MicroserviceFramework.Domain.Event
{
    public interface IDomainHandlerFactory
    {
        object Create(Type handlerType);
    }
}