using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MSFramework.AspNetCore;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResult>
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
		public async Task<ApiResult> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(command.OrderId);

			order.SetCancelledStatus();
			await _orderRepository.UnitOfWork.CommitAsync();
			return new ApiResult();
		}
	}
}