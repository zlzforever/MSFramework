using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	/// <summary>
	/// 领域事件分发器
	/// </summary>
	public interface IDomainEventDispatcher : IDisposable
	{
		/// <summary>
		/// 发布领域事件
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		Task DispatchAsync(DomainEvent @event);
	}
}