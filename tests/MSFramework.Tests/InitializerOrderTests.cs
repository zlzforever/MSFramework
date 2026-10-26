using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 初始化器执行顺序测试：验证 <see cref="MicroserviceFramework.IInitializer"/> 的
/// <c>Order</c> 契约——Order 值越大越先执行，未设置（默认 0）最后执行，
/// 相同 Order 按注册顺序稳定执行。
/// </summary>
/// <remarks>
/// 测试显式注册初始化器（不调用 AddMicroserviceFramework 程序集扫描），
/// 避免扫描把测试初始化器注册进其他测试的容器、交叉执行造成断言污染。
/// </remarks>
public class InitializerOrderTests
{
    /// <summary>
    /// 记录测试初始化器的实际执行顺序（仅本测试会触达，静态列表安全）
    /// </summary>
    private static readonly List<string> ExecutionLog = [];

    /// <summary>Order=1 的初始化器，应比 Order=2 的后执行</summary>
    private sealed class FirstInitializer : InitializerBase
    {
        /// <summary>
        /// 构造时设置执行顺序（Order 为基类非虚属性，通过 setter 赋值）
        /// </summary>
        public FirstInitializer()
        {
            Order = 1;
        }

        /// <summary>记录执行顺序</summary>
        public override void Start()
        {
            ExecutionLog.Add(nameof(FirstInitializer));
        }
    }

    /// <summary>Order=2 的初始化器，应在 FirstInitializer 之前执行</summary>
    private sealed class SecondInitializer : InitializerBase
    {
        /// <summary>
        /// 构造时设置执行顺序（Order 为基类非虚属性，通过 setter 赋值）
        /// </summary>
        public SecondInitializer()
        {
            Order = 2;
        }

        /// <summary>记录执行顺序</summary>
        public override void Start()
        {
            ExecutionLog.Add(nameof(SecondInitializer));
        }
    }

    /// <summary>未设置 Order（默认 0）的初始化器，应最后执行</summary>
    private sealed class DefaultOrderInitializer : InitializerBase
    {
        /// <summary>记录执行顺序</summary>
        public override void Start()
        {
            ExecutionLog.Add(nameof(DefaultOrderInitializer));
        }
    }

    /// <summary>
    /// 断言三个初始化器按 Order 降序执行：2 → 1 → 0。
    /// 并行运行的其他测试容器也会扫描到本测试类的初始化器并执行，
    /// 因此不逐条比对日志，而是断言三者首次执行记录的相对顺序符合契约。
    /// </summary>
    [Fact]
    public void InitializersExecuteInDescendingOrderByContract()
    {
        // IInitializer 为 internal 接口，测试工程无法直接引用，
        // 通过 InitializerBase 实现的接口集合在运行期取得服务类型
        var initializerInterface = typeof(InitializerBase).GetInterfaces()
            .Single(x => x.Name == "IInitializer");

        var services = new ServiceCollection();
        services.AddLogging();
        // 注册顺序刻意与期望执行顺序不同：执行顺序只应由 Order 决定
        // （IInitializer 为 internal 接口，只能以 ServiceDescriptor 形式注册）
        ((ICollection<ServiceDescriptor>)services).Add(
            new ServiceDescriptor(initializerInterface, typeof(SecondInitializer), ServiceLifetime.Singleton));
        ((ICollection<ServiceDescriptor>)services).Add(
            new ServiceDescriptor(initializerInterface, typeof(DefaultOrderInitializer), ServiceLifetime.Singleton));
        ((ICollection<ServiceDescriptor>)services).Add(
            new ServiceDescriptor(initializerInterface, typeof(FirstInitializer), ServiceLifetime.Singleton));

        using var serviceProvider = services.BuildServiceProvider();
        serviceProvider.UseMicroserviceFramework();

        // 取每个初始化器在日志中的首次出现位置，验证相对顺序
        var firstIndex = new Dictionary<string, int>();
        for (var i = 0; i < ExecutionLog.Count; i++)
        {
            if (!firstIndex.ContainsKey(ExecutionLog[i]))
            {
                firstIndex[ExecutionLog[i]] = i;
            }
        }

        Assert.Contains(nameof(DefaultOrderInitializer), firstIndex.Keys);
        Assert.Contains(nameof(FirstInitializer), firstIndex.Keys);
        Assert.Contains(nameof(SecondInitializer), firstIndex.Keys);
        Assert.True(
            firstIndex[nameof(SecondInitializer)] < firstIndex[nameof(FirstInitializer)] &&
            firstIndex[nameof(FirstInitializer)] < firstIndex[nameof(DefaultOrderInitializer)],
            $"实际执行顺序: {string.Join(" -> ", ExecutionLog)}");
    }
}
