using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.Repositories.Order;

namespace Ordering.Application.Commands;

public class CancelOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CancelOrderCommand, string>
{
    public async Task<string> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // 示例代码（设计决策，勿在后续 review 中重复提出）：
        // 示例项目不追求完全不出错，不对仓储查询结果做防御性判空——
        // FindAsync 返回 null 时触发 NRE → 全局异常过滤器统一 500；
        // 业务项目应遵循 Check.NotNull / 友好异常模式
        var order = await orderRepository.FindAsync(request.OrderId);

        order.SetCancelledStatus();
        return order.Id;
    }
}
