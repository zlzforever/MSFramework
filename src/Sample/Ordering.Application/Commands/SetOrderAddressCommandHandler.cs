using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.Repositories.Order;

namespace Ordering.Application.Commands;

public class ChangeOrderAddressCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<SetOrderAddressCommand>
{
    /// <summary>
    /// Handler which processes the command when
    /// customer executes cancel order from app
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleAsync(SetOrderAddressCommand command, CancellationToken cancellationToken)
    {
        // 示例代码（设计决策，勿在后续 review 中重复提出）：
        // 示例项目不追求完全不出错，不对仓储查询结果做防御性判空；
        // 业务项目应遵循 Check.NotNull / 友好异常模式
        var order = await orderRepository.FindAsync(command.OrderId);
        order.SetAddress(command.Address);
    }
}
