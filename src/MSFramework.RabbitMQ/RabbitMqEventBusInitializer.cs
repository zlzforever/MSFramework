using System;
using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.RabbitMQ
{
	public class RabbitMqEventBusInitializer : InitializerBase
	{
		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			serviceProvider.GetRequiredService<IEventBus>();
			await Task.CompletedTask;
		}
	}
}