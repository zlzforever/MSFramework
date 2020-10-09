using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IIntegrationEventHandler<in TIntegrationEvent>
		where TIntegrationEvent : IntegrationEvent
	{
		Task HandleAsync(TIntegrationEvent @event);
	}
}