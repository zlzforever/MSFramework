using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.LocalEvent;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// EventDescriptorStore 回归测试：
/// Freeze 顺序（先建快照置位、再清空）、GetList 防御性拷贝、幂等与冻结后拒绝注册
/// </summary>
public class EventDescriptorStoreTests
{
    private sealed class TestHandler
    {
        public Task HandleAsync(string @event, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestHandler2
    {
        public Task HandleAsync(string @event, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void Register_AfterFreeze_Throws()
    {
        var store = new EventDescriptorStore();
        store.Freeze();
        Assert.Throws<InvalidOperationException>(() =>
            store.Register(typeof(string), typeof(TestHandler)));
    }

    [Fact]
    public void Freeze_IsIdempotent()
    {
        var store = new EventDescriptorStore();
        store.Register(typeof(string), typeof(TestHandler));
        store.Freeze();
        store.Freeze();

        var handlers = store.GetList(typeof(string));
        Assert.Single(handlers);
    }

    /// <summary>
    /// 同一处理器类型重复注册时按 HandlerType 去重，不能因 InvokeAsync 委托引用不同而执行两次。
    /// </summary>
    [Fact]
    public void Register_DuplicateHandlerType_DoesNotDuplicateDescriptor()
    {
        var store = new EventDescriptorStore();
        store.Register(typeof(string), typeof(TestHandler));
        store.Register(typeof(string), typeof(TestHandler));

        Assert.Single(store.GetList(typeof(string)));
    }

    [Fact]
    public void GetList_BeforeFreeze_ReturnsDefensiveCopy()
    {
        // 未冻结时返回集合本体会在后续 Register 时触发"集合已修改"枚举异常——必须返回拷贝
        var store = new EventDescriptorStore();
        store.Register(typeof(string), typeof(TestHandler));

        var first = store.GetList(typeof(string));
        Assert.Single(first);

        store.Register(typeof(string), typeof(TestHandler2));

        // 对第一次返回的集合继续枚举不抛异常，且内容不受后续注册影响
        Assert.Single(first);
        Assert.Equal(2, store.GetList(typeof(string)).Count);
    }

    [Fact]
    public void GetList_AfterFreeze_ReturnsAllRegisteredDescriptors()
    {
        var store = new EventDescriptorStore();
        store.Register(typeof(string), typeof(TestHandler));
        store.Freeze();

        Assert.Single(store.GetList(typeof(string)));
        Assert.Empty(store.GetList(typeof(int)));
    }
}
