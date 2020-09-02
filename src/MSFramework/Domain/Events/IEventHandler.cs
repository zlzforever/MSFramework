using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Events
{
	public interface IEventHandler<in TEvent>
		where TEvent : class, IEvent
	{
		Task HandleAsync(TEvent @event);
	}
}