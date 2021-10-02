using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.RabbitMQ
{
	public class RabbitMqEventBusInitializer : InitializerBase
	{
		private readonly IServiceProvider _serviceProvider;

		public RabbitMqEventBusInitializer(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_serviceProvider.GetRequiredService<IEventBus>();
			return Task.CompletedTask;
		}
	}
}