using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.EventBus;
using Ordering.Application.Command;
using Ordering.Application.Event;

namespace Ordering.Application.EventHandler
{
	public class UserCheckoutAcceptedEventHandler : IEventHandler<UserCheckoutAcceptedEvent>
	{
		private readonly ILogger _logger;
		private readonly IMediator _mediator;
		private readonly IMSFrameworkSession _session;

		public UserCheckoutAcceptedEventHandler(IMSFrameworkSession session,
			IMediator mediator,
			ILogger<UserCheckoutAcceptedEventHandler> logger)
		{
			_logger = logger;
			_mediator = mediator;
			_session = session;
		}

		public async Task Handle(UserCheckoutAcceptedEvent @event)
		{
			await _mediator.Send(new CreateOrderCommand(@event.OrderItems.Select(x =>
					new CreateOrderCommand.OrderItemDTO
					{
						Discount = x.Discount,
						ProductId = x.ProductId,
						PictureUrl = x.PictureUrl,
						ProductName = x.ProductName,
						Units = x.Units,
						UnitPrice = x.UnitPrice
					}).ToList(), @event.UserId, @event.City, @event.Street,
				@event.State, @event.Country, @event.ZipCode, @event.Description));
		}
	}
}