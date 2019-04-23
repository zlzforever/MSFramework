using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.EventBus;
using Ordering.API.Application.Command;
using Ordering.API.Application.Event;

namespace Ordering.API.Application.EventHandler
{
	public class UserCheckoutAcceptedEventHandler : IEventHandler<UserCheckoutAcceptedEvent>
	{
		private readonly ICommandBus _commandBus;
		private readonly ILogger _logger;

		public UserCheckoutAcceptedEventHandler(ICommandBus commandBus,
			ILogger<UserCheckoutAcceptedEventHandler> logger)
		{
			_commandBus = commandBus;
			_logger = logger;
		}

		public async Task Handle(UserCheckoutAcceptedEvent @event)
		{
			var command = new CreateOrderCommand(@event.OrderItems, @event.UserId, @event.City, @event.Street,
				@event.State, @event.Country, @event.ZipCode, @event.Description);
			// IdentifiedCommand<> 必须保证只执行一次
			var result = await _commandBus.SendAsync(command);

			if (result)
			{
				_logger.LogInformation("----- CreateOrderCommand suceeded - RequestId: {RequestId}", @event.Id);
			}
			else
			{
				_logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", @event.Id);
			}
		}
	}
}