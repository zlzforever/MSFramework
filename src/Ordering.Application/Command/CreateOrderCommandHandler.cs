using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, IActionResult>
	{
		private readonly IOrderingRepository _orderRepository;

		public CreateOrderCommandHandler(IOrderingRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		/// <summary>
		/// Handler which processes the command when
		/// customer executes cancel order from app
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public async Task<IActionResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
		{
			var order = new Order(
				command.UserId,
				new Address(command.Street, command.City, command.State, command.Country, command.ZipCode),
				command.Description,
				command.OrderItems.Select(x => x.ToOrderItem()).ToList());
			await _orderRepository.InsertAsync(order);
			await _orderRepository.UnitOfWork.CommitAsync();
			return new OkResult();
		}
	}
}