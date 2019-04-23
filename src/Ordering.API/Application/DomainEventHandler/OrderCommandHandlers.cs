using MSFramework;
using MSFramework.CQRS;
using Ordering.API.Application.Command;
using Ordering.Domain.Aggregates;
using Ordering.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.API.Application.EventHandler
{
	public class OrderCommandHandlers : ICommandHandler<CreateOrderCommand>,
										ICommandHandler<ChangeOrderAddressCommand>,
										ICommandHandler<DeleteOrderCommand>
	{
		private readonly IOrdingRepository _repository;

		public OrderCommandHandlers(OrderingRepository repository)
		{
			_repository = repository;
		}

		public void Handle(CreateOrderCommand command)
		{
			var item = new Order(
				command.Id,
				command.ExpectedVersion,
				command.Description,
				command.Address,
				command.OrderItems.Select(x => new OrderItem()
				{
					OrderId = x.OrderId,
					StoreItemDescription = x.StoreItemDescription,
					StoreItemId = x.StoreItemId,
					StoreItemUrl = x.StoreItemUrl
				}).ToList());
			_repository.Insert(item);
		}
		
		public void Handle(ChangeOrderAddressCommand command)
		{
			Order item = _repository.Get(command.Id);
			item.ChangeAddress(command.NewAddress);
		}

		public void Handle(DeleteOrderCommand command)
		{
			Order item = _repository.Get(command.Id);
			if (item.Version != command.ExpectedVersion)
			{
				throw new MSFrameworkException("version validate failed");
			}
			item.Delete();
			
		}
	}
}
