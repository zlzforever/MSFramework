using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventBus : IDisposable
	{
		/// <summary>
		/// 发布事件
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task PublishAsync(dynamic @event);

		/// <summary>
		/// 发布事件
		/// </summary>
		/// <param name="event"></param>
		/// <typeparam name="TEvent"></typeparam>
		/// <returns></returns>
		Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase;

		/// <summary>
		/// 发布事件，若是事件对象则发布，返回 false
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task<bool> PublishIfEventAsync(dynamic @event);
	}
}