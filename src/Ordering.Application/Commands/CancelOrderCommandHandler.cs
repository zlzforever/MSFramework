using System.Threading;
using System.Threading.Tasks;
using MSFramework.Application;
using MSFramework.Domain;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class CancelOrderCommandHandler : IRequestHandler 
	{
		private readonly IOrderingRepository _orderRepository;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public CancelOrderCommandHandler(IOrderingRepository orderRepository, IUnitOfWorkManager unitOfWorkManager)
		{
			_orderRepository = orderRepository;
			_unitOfWorkManager = unitOfWorkManager;
		}

		public async Task HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(request.OrderId);

			order.SetCancelledStatus();
			await _unitOfWorkManager.CommitAsync();
		}

		public Task HandleAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IRequest
		{
			throw new System.NotImplementedException();
		}
	}
}