using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Application;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.Application.Command
{
	public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
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
		public async Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
		{
			var order = new Order(
				command.UserId,
				new Address(command.Street, command.City, command.State, command.Country, command.ZipCode),
				command.Description,
				command.OrderItems.Select(x => x.ToOrderItem()).ToList());
			await _orderRepository.InsertAsync(order);
			await _unitOfWorkManager.CommitAsync();
		}
	}
}