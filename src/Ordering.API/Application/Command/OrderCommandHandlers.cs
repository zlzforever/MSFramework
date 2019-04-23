using System;
using System.Linq;
using System.Threading.Tasks;
using MSFramework;
using MSFramework.Command;
using MSFramework.Domain.Repository;
using MSFramework.Security;
using Ordering.API.Application.Event;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Application.Command
{
	public class OrderCommandHandlers : ICommandHandler<CreateOrderCommand>,
		ICommandHandler<ChangeOrderAddressCommand>,
		ICommandHandler<DeleteOrderCommand>
	{
		private readonly IRepository<Order, Guid> _repository;
		private readonly ICurrentUser _currentUser;

		public OrderCommandHandlers(IRepository<Order, Guid> repository, ICurrentUser currentUser)
		{
			_repository = repository;
			_currentUser = currentUser;
		}

		public async Task ExecuteAsync(CreateOrderCommand command)
		{
			var order = new Order(
				command.UserId,
				new Address(command.Street, command.City, command.State, command.Country, command.ZipCode),
				command.Description,
				command.OrderItems.Select(x => x.ToOrderItem()).ToList());
			order.RegisterDomainEvent(new OrderStartedEvent(_currentUser.UserId, order.Id));
			await _repository.InsertAsync(order);
		}

		public async Task ExecuteAsync(ChangeOrderAddressCommand command)
		{
			var item = await _repository.GetAsync(command.OrderId);
			item.ChangeAddress(command.NewAddress);
		}

		public async Task ExecuteAsync(DeleteOrderCommand command)
		{
			var item = await _repository.GetAsync(command.OrderId);
			if (item.Version != command.Version)
			{
				throw new MSFrameworkException("version validate failed");
			}

			item.Delete();
		}
	}
}