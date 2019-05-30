using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class ChangeOrderAddressCommandHandler : IRequestHandler<ChangeOrderAddressCommand, bool>
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
		/// <returns></returns>
		public async Task<bool> Handle(ChangeOrderAddressCommand command, CancellationToken cancellationToken)
		{
			var orderToUpdate = await _orderRepository.GetAsync(command.OrderId);
			if (orderToUpdate == null)
			{
				return false;
			}

			orderToUpdate.ChangeAddress(command.NewAddress);
			return await _orderRepository.UnitOfWork.CommitAsync();
		}
	}
}