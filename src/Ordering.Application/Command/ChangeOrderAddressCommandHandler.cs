using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class ChangeOrderAddressCommandHandler : IRequestHandler<ChangeOrderAddressCommand, IActionResult>
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
		public async Task<IActionResult> Handle(ChangeOrderAddressCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(command.OrderId);
			if (order == null)
			{
				return new BadRequestResult();
			}

			order.ChangeAddress(command.NewAddress);
			await _orderRepository.UnitOfWork.CommitAsync();
			return new OkResult();
		}
	}
}