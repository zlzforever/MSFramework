using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MSFramework.EventBus;
using Ordering.API.Application.Command;

namespace Ordering.API.Application.EventHandler
{
	public class UserCheckoutAcceptedIntegrationEventHandler : IEventHandler<UserCheckoutAcceptedIntegrationEvent>
	{
		private readonly IMediator _mediator;
		private readonly ILogger<UserCheckoutAcceptedIntegrationEventHandler> _logger;

		public UserCheckoutAcceptedIntegrationEventHandler(IMemoryCache cache,
			IMediator mediator,
			ILogger<UserCheckoutAcceptedIntegrationEventHandler> logger)
		{
			_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Handle(UserCheckoutAcceptedIntegrationEvent @event)
		{
			_logger.LogInformation(
				"----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
				@event.Id, Assembly.GetEntryAssembly().GetName().Name, @event);

			var result = false;

			if (@event.RequestId != Guid.Empty)
			{
				var createOrderCommand = new CreateOrderCommand(@event.Basket.Items, @event.UserId,
					@event.UserName, @event.City, @event.Street,
					@event.State, @event.Country, @event.ZipCode,
					@event.CardNumber, @event.CardHolderName, @event.CardExpiration,
					@event.CardSecurityNumber, @event.CardTypeId);

				// TODO: 根据 requestId 做去复，防止重复提交
				result = await _mediator.Send(createOrderCommand);

				if (result)
				{
					_logger.LogInformation("----- CreateOrderCommand suceeded - RequestId: {RequestId}",
						@event.RequestId);
				}
				else
				{
					_logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", @event.RequestId);
				}
			}
			else
			{
				_logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", @event);
			}
		}
	}
}