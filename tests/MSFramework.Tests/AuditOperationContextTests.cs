using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// <see cref="AuditOperationContext"/>（AsyncLocal 承载审计操作）行为测试：
/// 验证审计操作随执行流流转、跨执行流隔离（不串扰）、清理后恢复 null 的核心语义，
/// 这些语义是「池化 DbContext 下残留订阅读错对象跨请求污染」修复方案的基础。
/// </summary>
public class AuditOperationContextTests
{
    /// <summary>
    /// 构建测试用审计操作
    /// </summary>
    /// <returns>审计操作实例</returns>
    private static AuditOperation CreateOperation()
    {
        return new AuditOperation("/orders", "ua", "1.2.3.4", "iPhone", "device-1",
            null, null, "trace-1", "POST");
    }

    /// <summary>
    /// 审计操作设置后必须随 await 延续流转，在异步续延中仍可读取（请求执行流延续到 SaveChanges 的前提）
    /// </summary>
    [Fact]
    public async Task Value_FlowsAcrossAwaitContinuations()
    {
        var operation = CreateOperation();
        AuditOperationContext.Value = operation;
        try
        {
            // Task.Yield/Delay 强制发生线程切换或异步挂起，验证 AsyncLocal 值随 ExecutionContext 流转
            await Task.Yield();
            await Task.Delay(10);

            Assert.Same(operation, AuditOperationContext.Value);
        }
        finally
        {
            AuditOperationContext.Value = null;
        }
    }

    /// <summary>
    /// 清理（置 null）后当前执行流必须读取到 null，验证请求结束清理语义生效
    /// </summary>
    [Fact]
    public async Task Value_AfterSetToNull_ReturnsNull()
    {
        AuditOperationContext.Value = CreateOperation();
        AuditOperationContext.Value = null;

        Assert.Null(AuditOperationContext.Value);

        // 清理后在异步续延中同样为 null（随 ExecutionContext 流转的 null 状态）
        await Task.Yield();
        Assert.Null(AuditOperationContext.Value);
    }

    /// <summary>
    /// 子执行流（Task.Run 独立上下文）内的写入不得回流到父执行流，
    /// 保证各请求各自的审计操作互不可见
    /// </summary>
    [Fact]
    public async Task Value_ChildContextWrite_DoesNotFlowBackToParent()
    {
        var operation = CreateOperation();

        // 父执行流此时应为 null（xUnit 用例间不共享 AsyncLocal 值）
        await Task.Run(() =>
        {
            Assert.Null(AuditOperationContext.Value);
            AuditOperationContext.Value = operation;
            Assert.Same(operation, AuditOperationContext.Value);
        });

        // 子执行流写入不影响父执行流
        Assert.Null(AuditOperationContext.Value);
    }

    /// <summary>
    /// 并发执行流各自设置审计操作后互不污染：并行请求场景下每个请求只能读到自己的审计操作
    /// </summary>
    [Fact]
    public async Task Value_ConcurrentFlows_DoNotPolluteEachOther()
    {
        var operationA = CreateOperation();
        var operationB = CreateOperation();

        var flowA = Task.Run(async () =>
        {
            AuditOperationContext.Value = operationA;
            await Task.Delay(200);
            var observed = AuditOperationContext.Value;
            AuditOperationContext.Value = null;
            return observed;
        });
        var flowB = Task.Run(async () =>
        {
            AuditOperationContext.Value = operationB;
            await Task.Delay(200);
            var observed = AuditOperationContext.Value;
            AuditOperationContext.Value = null;
            return observed;
        });

        var (observedA, observedB) = await Task.WhenAll(flowA, flowB) switch
        {
            var results when results.Length == 2 => (results[0], results[1]),
            _ => (null, null)
        };

        Assert.Same(operationA, observedA);
        Assert.Same(operationB, observedB);
        Assert.NotSame(observedA, observedB);
    }
}
