using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Domain.Repositories.Order;

namespace Ordering.Application.Commands;

public class CreateOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, string>
{
    /// <summary>
    /// Handler which processes the command when
    /// customer executes cancel order from app
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            command.UserId,
            new Address(command.Street, command.City, command.State, command.Country, command.ZipCode),
            command.Description
        );

        // await _orderRepository.LoadAsync(order, x => x.Items);

        foreach (var item in command.OrderItems)
        {
            order.AddItem(item.ProductId, item.ProductName, item.PictureUrl, item.UnitPrice,
                item.Units, item.Discount);
        }

        await orderRepository.AddAsync(order);
        return order.Id.ToString();
    }
}
