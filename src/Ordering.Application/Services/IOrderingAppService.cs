using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.Application;
using Ordering.Application.DTO;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Application.Services
{
	public interface IOrderingAppService : IApplicationService
	{
		Task DeleteOrder(Guid orderId);

		Task ChangeOrderAddress(ChangeOrderAddressDTO dto);

		Task CreateOrder(CreateOrderDTO dto);
	}
}