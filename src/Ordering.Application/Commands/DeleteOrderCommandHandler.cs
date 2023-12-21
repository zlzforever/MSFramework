using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands;

public class DeleteOrderCommandHandler(IOrderingRepository orderRepository) : IRequestHandler<DeleteOrderCommand>
{
    /// <summary>
    /// Handler which processes the command when
    /// customer executes cancel order from app
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindAsync(command.OrderId);
        if (order == null)
        {
            return;
        }

        await orderRepository.DeleteAsync(order);
    }
}
