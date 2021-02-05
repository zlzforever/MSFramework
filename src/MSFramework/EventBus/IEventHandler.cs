using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventHandler<in TEvent> : IDisposable
		where TEvent : class
	{
		Task HandleAsync(TEvent @event);
	}
}