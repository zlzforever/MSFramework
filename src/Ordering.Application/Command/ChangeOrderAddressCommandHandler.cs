using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MSFramework.Http;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class ChangeOrderAddressCommandHandler : IRequestHandler<ChangeOrderAddressCommand, ApiResult>
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
		public async Task<ApiResult> Handle(ChangeOrderAddressCommand command, CancellationToken cancellationToken)
		{
			var order = await _orderRepository.GetAsync(command.OrderId);
			if (order == null)
			{
				return new ApiResult(false, null, "无效的订单号");
			}

			order.ChangeAddress(command.NewAddress);
			await _orderRepository.UnitOfWork.CommitAsync();
			return new ApiResult();
		}
	}
}