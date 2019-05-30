using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
	{
		private readonly IOrderingRepository _orderRepository;

		public CancelOrderCommandHandler(IOrderingRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		/// <summary>
		/// Handler which processes the command when
		/// customer executes cancel order from app
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public async Task<bool> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
		{
			var orderToUpdate = await _orderRepository.GetAsync(command.OrderId);
			if(orderToUpdate == null)
			{
				return false;
			}

			orderToUpdate.SetCancelledStatus();
			return await _orderRepository.UnitOfWork.CommitAsync();
		}
	}
}