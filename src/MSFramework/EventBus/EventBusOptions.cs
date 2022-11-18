using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public class EventBusOptions
{
    internal readonly List<Func<IServiceProvider, Task>> BeforeFunctions = new();
    internal readonly List<Func<IServiceProvider, Task>> AfterFunctions = new();

    public void AddBeforeInterceptors(params Func<IServiceProvider, Task>[] functions)
    {
        BeforeFunctions.AddRange(functions);
    }

    public void AddAfterInterceptors(params Func<IServiceProvider, Task>[] functions)
    {
        AfterFunctions.AddRange(functions);
    }
}
