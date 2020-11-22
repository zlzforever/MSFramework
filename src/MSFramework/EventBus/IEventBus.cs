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
		Task PublishAsync(EventBase @event);
	}
}