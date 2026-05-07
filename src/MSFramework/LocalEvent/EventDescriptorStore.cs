using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;
using System.Threading;

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

    // 冻结后的不可变字典（零开销并发读）
    private FrozenDictionary<Type, EventDescriptor[]> _frozen;

    // 标记是否已冻结（volatile 保证可见性，不需要 lock）
    private volatile bool _frozenFlag;

    /// <summary>
    /// 注册事件处理器（仅构建期调用）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="handlerType">处理器类型</param>
    /// <exception cref="ArgumentException">处理器未实现 HandleAsync 方法</exception>
    /// <exception cref="InvalidOperationException">已冻结后调用</exception>
    public void Register(Type eventType, Type handlerType)
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

        if (!_mutable.ContainsKey(eventType))
        {
            _mutable.Add(eventType, new HashSet<EventDescriptor>());
        }

        _mutable[eventType].Add(new EventDescriptor(handlerType, method));
    }

    /// <summary>
    /// 冻结存储，转为不可变集合。
    /// 必须在所有 Register() 调用完成后、GetList() 调用前执行。
    /// 只能调用一次。
    /// </summary>
    public void Freeze()
    {
        if (_frozenFlag)
        {
            return; // 幂等
        }

        // 将可变字典转换为 FrozenDictionary（.NET 8+ 零开销只读集合）
        _frozen = _mutable.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray());

        // 清除可变字典，帮助 GC
        _mutable.Clear();

        _frozenFlag = true;
    }

    /// <summary>
    /// 获取指定事件类型的所有处理器描述符（线程安全）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>处理器描述符集合，无匹配时返回空数组</returns>
    public IReadOnlyCollection<EventDescriptor> GetList(Type eventType)
    {
        if (!_frozenFlag)
        {
            // 未冻结时，返回可变字典的快照（构造期过渡行为）
            if (_mutable.TryGetValue(eventType, out var set))
            {
                return set;
            }

            return Array.Empty<EventDescriptor>();
        }

        // 已冻结：FrozenDictionary 的分支预测友好查找
        if (_frozen.TryGetValue(eventType, out var array))
        {
            return array;
        }

        return Array.Empty<EventDescriptor>();
    }
}
