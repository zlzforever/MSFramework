using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Application;
using MSFramework.Common;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Application.Commands
{
	public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ObjectId>
	{
		private readonly IOrderingRepository _orderRepository;
		private readonly IUnitOfWorkManager _unitOfWorkManager;

		public CreateOrderCommandHandler(IOrderingRepository orderRepository, IUnitOfWorkManager unitOfWorkManager)
		{
			_orderRepository = orderRepository;
			_unitOfWorkManager = unitOfWorkManager;
		}

		/// <summary>
		/// Handler which processes the command when
		/// customer executes cancel order from app
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public async Task<ObjectId> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
		{
			var order = new Order(
				command.UserId,
				new Address(command.Street, command.City, command.State, command.Country, command.ZipCode),
				command.Description,
				command.OrderItems.Select(x => x.ToOrderItem()).ToList());
			await _orderRepository.InsertAsync(order);
			return order.Id;
		}
	}
}