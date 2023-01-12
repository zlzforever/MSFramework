using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public class EventBusOptions
{
    internal readonly List<Func<IServiceProvider, Task>> BeforeDelegates = new();
    internal readonly List<Func<IServiceProvider, Task>> AfterDelegates = new();

    public void AddBeforeInterceptors(params Func<IServiceProvider, Task>[] delegates)
    {
        BeforeDelegates.AddRange(delegates);
    }

    public void AddAfterInterceptors(params Func<IServiceProvider, Task>[] delegates)
    {
        AfterDelegates.AddRange(delegates);
    }
}
