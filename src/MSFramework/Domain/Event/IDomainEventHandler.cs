using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	/// <summary>
	/// 领域事件处理器
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	public interface IDomainEventHandler<in TEvent> : IDisposable
		where TEvent : DomainEvent
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task HandleAsync(TEvent @event);
	}
}