using System.Threading;
using System.Threading.Tasks;
using MSFramework.Application;
using MSFramework.Domain;
using MSFramework.Shared;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ObjectId>
	{
		private readonly IOrderingRepository _orderRepository;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public CancelOrderCommandHandler(IOrderingRepository orderRepository, IUnitOfWorkManager unitOfWorkManager)
		{
			_orderRepository = orderRepository;
			_unitOfWorkManager = unitOfWorkManager;
		}

		public async Task<ObjectId> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(request.OrderId);

			order.SetCancelledStatus();
			await _unitOfWorkManager.CommitAsync();
			return ObjectId.NewId();
		}
	}
}