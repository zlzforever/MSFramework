using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Template.Application.Other.IntegrationEvents;

public class SubscribeService : ICapSubscribe
{
	private readonly ILogger<SubscribeService> _logger;

	public SubscribeService(ILogger<SubscribeService> logger)
	{
		_logger = logger;
	}

	[CapSubscribe("Template.Application.Product.SubscribeService.ProjectCreatedIntegrationEvent")]
	public void OnProjectCreated(Subscribe.ProjectCreatedEvent @event)
	{
		_logger.LogInformation($"Project {@event.Id} created");
	}
}