using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
	{
		private readonly IOrderingRepository _orderRepository;

		public DeleteOrderCommandHandler(IOrderingRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		/// <summary>
		/// Handler which processes the command when
		/// customer executes cancel order from app
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public async Task<bool> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
		{
			var orderToUpdate = await _orderRepository.GetAsync(command.OrderId);
			if (orderToUpdate == null)
			{
				return false;
			}

			await _orderRepository.DeleteAsync(orderToUpdate);
			return await _orderRepository.UnitOfWork.CommitAsync();
		}
	}
}