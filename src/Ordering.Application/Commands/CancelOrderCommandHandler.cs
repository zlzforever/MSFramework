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
        var order = await orderRepository.FindAsync(request.OrderId);

        order.SetCancelledStatus();
        return order.Id;
    }
}
