using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application.CQRS;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class ChangeOrderAddressCommandHandler : ICommandHandler<ChangeOrderAddressCommand>
	{
		private readonly IOrderingRepository _orderRepository;

		public ChangeOrderAddressCommandHandler(IOrderingRepository orderRepository)
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
		public async Task HandleAsync(ChangeOrderAddressCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.FindAsync(command.OrderId);
			order?.ChangeAddress(command.NewAddress);
		}
	}
}