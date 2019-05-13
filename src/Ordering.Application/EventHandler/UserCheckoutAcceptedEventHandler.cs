using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.EventBus;
using Ordering.Application.DTO;
using Ordering.Application.Event;
using Ordering.Application.Services;

namespace Ordering.Application.EventHandler
{
	public class UserCheckoutAcceptedEventHandler : IEventHandler<UserCheckoutAcceptedEvent>
	{
		private readonly IOrderingAppService _orderingAppService;
		private readonly ILogger _logger;

		public UserCheckoutAcceptedEventHandler(IOrderingAppService orderingAppService,
			ILogger<UserCheckoutAcceptedEventHandler> logger)
		{
			_orderingAppService = orderingAppService;
			_logger = logger;
		}

		public async Task Handle(UserCheckoutAcceptedEvent @event)
		{
			var dto = new CreateOrderDTO(@event.OrderItems, @event.UserId, @event.City, @event.Street,
				@event.State, @event.Country, @event.ZipCode, @event.Description);
			// IdentifiedCommand<> 必须保证只执行一次
			await _orderingAppService.CreateOrder(dto);
		}
	}
}