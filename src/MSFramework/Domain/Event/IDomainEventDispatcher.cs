using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	/// <summary>
	/// 领域事件分发器
	/// 领域事件通知其它聚合做出变化，这些变化需要和发出者的变化原子性的更新到数据库，所以此接口的生命周期必须为 Scoped
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