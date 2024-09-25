using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.Repositories.Order;

namespace Ordering.Application.Commands;

public class ChangeOrderAddressCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<ChangeOrderAddressCommand>
{
    /// <summary>
    /// Handler which processes the command when
    /// customer executes cancel order from app
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleAsync(ChangeOrderAddressCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindAsync(command.OrderId);
        order?.SetAddress(command.NewAddress);
    }
}
