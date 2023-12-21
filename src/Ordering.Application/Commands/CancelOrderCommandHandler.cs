using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands;

public class CancelOrderCommandHandler(IOrderingRepository orderRepository)
    : IRequestHandler<CancelOrderCommand, ObjectId>
{
    public async Task<ObjectId> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindAsync(request.OrderId);

        order.SetCancelledStatus();
        return ObjectId.GenerateNewId();
    }
}
