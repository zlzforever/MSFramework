using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, IActionResult>
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
		public async Task<IActionResult> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
		{
			var orderToUpdate = await _orderRepository.GetAsync(command.OrderId);

			orderToUpdate.SetCancelledStatus();
			await _orderRepository.UnitOfWork.CommitAsync();
			return new OkResult();
		}
	}
}