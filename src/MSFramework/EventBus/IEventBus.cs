using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventBus
	{
		/// <summary>
		/// 发布事件
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task PublishAsync(Event @event);

		/// <summary>
		/// 订阅事件
		/// </summary>
		/// <typeparam name="TEvent"></typeparam>
		/// <typeparam name="TEventHandler"></typeparam>
		/// <returns></returns>
		Task SubscribeAsync<TEvent, TEventHandler>()
			where TEvent : Event
			where TEventHandler : IEventHandler<TEvent>;

		/// <summary>
		/// 订阅事件
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="handlerType"></param>
		/// <returns></returns>
		Task SubscribeAsync(Type eventType, Type handlerType);

		/// <summary>
		/// 取消订阅
		/// </summary>
		/// <typeparam name="TEvent"></typeparam>
		/// <typeparam name="TEventHandler"></typeparam>
		void Unsubscribe<TEvent, TEventHandler>()
			where TEventHandler : IEventHandler<TEvent>
			where TEvent : Event;
	}
}