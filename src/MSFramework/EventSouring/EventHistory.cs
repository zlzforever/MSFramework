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
		public string EventType { get; set; }

		/// <summary>
		/// 序列化的领域事件
		/// </summary>
		[Required]
		public string Event { get; set; }

		/// <summary>
		/// 聚合根标识
		/// </summary>
		[Required]
		[StringLength(255)]
		public string AggregateRootId { get; set; }

		/// <summary>
		/// 版本号
		/// </summary>
		[Required]
		public int Version { get; set; }

		/// <summary>
		/// 创建者
		/// </summary>
		[StringLength(255)]
		public string Creator { get; set; }
		
		[StringLength(255)]
		public string CreatorId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime Timestamp { get; set; }

		public EventHistory()
		{
			Timestamp = DateTime.UtcNow;
		}

		public EventHistory(IAggregateRootChangedEvent @event) : this()
		{
			EventType = @event.GetType().AssemblyQualifiedName;
			Version = @event.Version;
			Event = Singleton<IJsonConvert>.Instance.SerializeObject(@event);
			AggregateRootId = @event.GetAggregateRootId();
			Creator = @event.Creator;
		}

		public IAggregateRootChangedEvent ToDomainEvent()
		{
			return (IAggregateRootChangedEvent) Singleton<IJsonConvert>.Instance.DeserializeObject(Event,
				Type.GetType(EventType));
		}
	}
}