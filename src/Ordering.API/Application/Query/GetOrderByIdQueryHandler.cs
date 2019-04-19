using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.AggregateRoot.Order;
using Ordering.Domain.Repository;

namespace Ordering.API.Application.Query
{
	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order>
	{
		private readonly IOrderingRepository _orderRepository;

		public GetOrderByIdQueryHandler(IOrderingRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<Order> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			return await _orderRepository.GetAsync(request.OrderId);
		}
	}
}