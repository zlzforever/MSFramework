using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.Repositories.Order;

namespace Ordering.Application.Commands;

public class DeleteOrderItemCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<DeleteOrderItemCommand>
{
    public async Task HandleAsync(DeleteOrderItemCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindAsync(command.OrderId);
        if (order == null)
        {
            return;
        }

        order.DeleteItem(command.OrderItemId);
    }
}
