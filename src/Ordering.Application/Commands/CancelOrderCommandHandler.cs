using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Application.CQRS.Command;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Shared;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, ObjectId>, IScopeDependency
	{
		private readonly IOrderingRepository _orderRepository;

		public CancelOrderCommandHandler(IOrderingRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<ObjectId> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(request.OrderId);

			order.SetCancelledStatus();
			return ObjectId.NewId();
		}
	}
}