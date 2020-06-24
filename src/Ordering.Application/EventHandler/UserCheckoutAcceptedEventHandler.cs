using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.Domain.Event;
using Ordering.Application.Event;

namespace Ordering.Application.EventHandler
{
	public class UserCheckoutAcceptedEventHandler : EventBus.IEventHandler<UserCheckoutAcceptedEvent>
	{
		private readonly ILogger _logger;
		private readonly IEventMediator _mediator;
		private readonly ISession _session;

		public UserCheckoutAcceptedEventHandler(ISession session,
			IEventMediator mediator,
			ILogger<UserCheckoutAcceptedEventHandler> logger)
		{
			_logger = logger;
			_mediator = mediator;
			_session = session;
		}

		public async Task HandleAsync(UserCheckoutAcceptedEvent @event)
		{
			// await _mediator.Send(new CreateOrderCommand(@event.OrderItems.Select(x =>
			// 		new CreateOrderCommand.OrderItemDTO
			// 		{
			// 			Discount = x.Discount,
			// 			ProductId = x.ProductId,
			// 			PictureUrl = x.PictureUrl,
			// 			ProductName = x.ProductName,
			// 			Units = x.Units,
			// 			UnitPrice = x.UnitPrice
			// 		}).ToList(), @event.UserId, @event.City, @event.Street,
			// 	@event.State, @event.Country, @event.ZipCode, @event.Description));
		}
	}
}