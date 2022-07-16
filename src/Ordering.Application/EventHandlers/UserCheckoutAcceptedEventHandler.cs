using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;

namespace Ordering.Application.EventHandlers
{
    public class UserCheckoutAcceptedEventHandler : IEventHandler<UserCheckoutAcceptedEvent>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly ISession _session;

        public UserCheckoutAcceptedEventHandler(ISession session,
            IMediator mediator,
            ILogger<UserCheckoutAcceptedEventHandler> logger)
        {
            _logger = logger;
            _mediator = mediator;
            _session = session;
        }

        public Task HandleAsync(UserCheckoutAcceptedEvent @event)
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
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}