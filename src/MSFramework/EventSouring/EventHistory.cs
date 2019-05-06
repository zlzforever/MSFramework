using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Serialization;

namespace MSFramework.EventSouring
{
	public class EventHistory : EntityBase<long>
	{
		/// <summary>
		/// 领域事件类型
		/// </summary>
		[Required]
		[StringLength(255)]
		public string EventType { get; }

		/// <summary>
		/// 序列化的领域事件
		/// </summary>
		[Required]
		public string Event { get; }

		/// <summary>
		/// 聚合根标识
		/// </summary>
		[Required]
		[StringLength(255)]
		public string AggregateRootId { get; }

		/// <summary>
		/// 版本号
		/// </summary>
		[Required]
		public int Version { get; }

		/// <summary>
		/// 创建者
		/// </summary>
		[StringLength(255)] 
		public string Creator { get; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime Timestamp { get; }

		public EventHistory(IAggregateRootChangedEvent @event)
		{
			EventType = @event.GetType().AssemblyQualifiedName;
			Version = @event.Version;
			Event = Singleton<IJsonConvert>.Instance.SerializeObject(@event);
			AggregateRootId = @event.GetAggregateRootId();
			Timestamp = DateTime.UtcNow;
			Creator = @event.Creator;
		}

		public IAggregateRootChangedEvent ToDomainEvent()
		{
			return (IAggregateRootChangedEvent) Singleton<IJsonConvert>.Instance.DeserializeObject(Event, Type.GetType(EventType));
		}
	}
}