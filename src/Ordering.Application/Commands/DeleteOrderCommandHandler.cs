using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Application.CQRS.Command;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
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
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(command.OrderId);
			if (order == null)
			{
				return;
			}

			await _orderRepository.DeleteAsync(order);
		}
	}
}