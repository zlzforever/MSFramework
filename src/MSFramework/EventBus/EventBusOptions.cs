using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public class EventBusOptions
{
    internal readonly List<Func<IServiceProvider, object, Task>> BeforeDelegates = new();
    internal readonly List<Func<IServiceProvider, object, Task>> AfterDelegates = new();

    public void AddBeforeInterceptor(Func<IServiceProvider, object, Task> @delegate)
    {
        BeforeDelegates.Add(@delegate);
    }

    public void AddAfterInterceptor(Func<IServiceProvider, object, Task> @delegate)
    {
        AfterDelegates.Add(@delegate);
    }
}
