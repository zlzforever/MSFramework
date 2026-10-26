using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件处理器描述符：处理器类型与注册期编译好的 <c>HandleAsync</c> 调用委托。
/// <para>
/// 委托在注册期（<see cref="EventDescriptorStore.Register"/>）一次性编译
/// （经 <see cref="HandlerDelegateBuilder"/>，与 Mediator 共用同一基建），
/// 派发期零反射调用、零 <c>object[]</c> 装箱分配——
/// 旧实现每次派发 <c>MethodInfo.Invoke</c> + 数组装箱，高频事件路径开销显著
/// </para>
/// </summary>
/// <param name="HandlerType">事件处理器类型</param>
/// <param name="InvokeAsync">编译后的调用委托：（handler 实例, 事件, 取消令牌）→ Task</param>
public record EventDescriptor(Type HandlerType,
    Func<object, object, CancellationToken, Task> InvokeAsync);
