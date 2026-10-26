using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;
using System.Threading;
using MicroserviceFramework.Mediator;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 事件处理器描述符存储
/// 两阶段设计：
///   1. 构建期：通过 Register() 注册所有描述符
///   2. 运行期：Freeze() 后转为 FrozenDictionary，GetList() 零锁并发读取
/// </summary>
public class EventDescriptorStore
{
    // 构建期可变字典（仅在 Freeze 前使用）
    private readonly Dictionary<Type, HashSet<EventDescriptor>> _mutable =
        new();

    // 注册/冻结共用锁：消除"Register 通过冻结前检查、Freeze 已清空字典"竞态下
    // 注册项静默丢失的窗口（启动期调用，锁开销可忽略）
    private readonly object _lock = new();

    // 冻结后的不可变字典（零开销并发读）
    private FrozenDictionary<Type, EventDescriptor[]> _frozen;

    // 标记是否已冻结（volatile 保证可见性，不需要 lock）
    private volatile bool _frozenFlag;

    /// <summary>
    /// 注册事件处理器（仅构建期调用）。
    /// 注册期一次性编译 HandleAsync 调用委托（与 Mediator 共用 <see cref="HandlerDelegateBuilder"/>），
    /// 派发期零反射零装箱
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="handlerType">处理器类型</param>
    /// <exception cref="ArgumentException">处理器未实现 HandleAsync 方法</exception>
    /// <exception cref="MicroserviceFrameworkException">HandleAsync 返回类型不是 Task（如 ValueTask）时抛出</exception>
    /// <exception cref="InvalidOperationException">已冻结后调用</exception>
    public void Register(Type eventType, Type handlerType)
    {
        lock (_lock)
        {
            if (_frozenFlag)
            {
                throw new InvalidOperationException(
                    "EventDescriptorStore 已冻结，不允许运行时注册处理器。");
            }

            var method = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)]);
            if (method == null)
            {
                throw new ArgumentException(
                    $"事件处理器 {handlerType.FullName} 没有实现事件 {eventType.FullName} 的处理方法");
            }

            // 注册期编译强类型调用委托（零反射派发）；HandleAsync 返回类型非 Task 时
            // 在注册期 fail-fast，而非派发期静默跳过（不执行、不保存、无日志）
            if (!_mutable.ContainsKey(eventType))
            {
                _mutable.Add(eventType, new HashSet<EventDescriptor>());
            }

            // 以 HandlerType 去重：EventDescriptor 是 record，但 InvokeAsync 每次注册都会
            // 编译出新的委托实例，直接依赖 record 自动相等会导致同一处理器重复注册后执行两次
            var descriptors = _mutable[eventType];
            if (descriptors.Any(d => d.HandlerType == handlerType))
            {
                return;
            }

            descriptors.Add(new EventDescriptor(handlerType,
                HandlerDelegateBuilder.Build(handlerType, method, eventType)));
        }
    }

    /// <summary>
    /// 冻结存储，转为不可变集合。
    /// 必须在所有 Register() 调用完成后、GetList() 调用前执行。
    /// 只能调用一次（幂等）。
    /// </summary>
    public void Freeze()
    {
        lock (_lock)
        {
            if (_frozenFlag)
            {
                return; // 幂等
            }

            // 先构建冻结快照并置位标志，再清空可变字典：
            // 若先 Clear 后置位，存在"_mutable 已清空、GetList 仍走未冻结分支返回空集合"的
            // 竞态窗口，窗口内的事件会静默丢失（无处理器、不报错）
            _frozen = _mutable.ToFrozenDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());
            _frozenFlag = true;

            // 清除可变字典，帮助 GC
            _mutable.Clear();
        }
    }

    /// <summary>
    /// 获取指定事件类型的所有处理器描述符（线程安全）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>处理器描述符集合，无匹配时返回空数组</returns>
    public IReadOnlyCollection<EventDescriptor> GetList(Type eventType)
    {
        // 已冻结：走零锁快路径（volatile 读保证冻结后的可见性）
        if (_frozenFlag)
        {
            return _frozen.TryGetValue(eventType, out var array)
                ? array
                : Array.Empty<EventDescriptor>();
        }

        // 未冻结（构造期过渡行为）：持锁返回防御性拷贝——
        // 返回 HashSet 本体会在后续 Register/Freeze 修改集合时触发"集合已修改"枚举异常
        lock (_lock)
        {
            if (_frozenFlag)
            {
                return _frozen.TryGetValue(eventType, out var array)
                    ? array
                    : Array.Empty<EventDescriptor>();
            }

            return _mutable.TryGetValue(eventType, out var set)
                ? set.ToArray()
                : Array.Empty<EventDescriptor>();
        }
    }
}
