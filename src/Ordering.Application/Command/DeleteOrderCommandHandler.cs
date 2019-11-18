using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MSFramework.AspNetCore;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, ApiResult>
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
		public async Task<ApiResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(command.OrderId);
			if (order == null)
			{
				return new FailedApiResult("无效的订单号");
			}

			await _orderRepository.DeleteAsync(order);
			await _orderRepository.UnitOfWork.CommitAsync();
			return new ApiResult();
		}
	}
}