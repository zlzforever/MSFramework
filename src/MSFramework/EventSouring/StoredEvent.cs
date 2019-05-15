using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.EventBus;
using MSFramework.Serialization;

namespace MSFramework.EventSouring
{
	public class StoredEvent : EntityBase<long>
	{
		/// <summary>
		/// 领域事件类型
		/// </summary>
		[Required]
		[StringLength(255)]
		public string EventType { get; private set; }

		/// <summary>
		/// 序列化的领域事件
		/// </summary>
		[Required]
		public string Event { get; private set; }

		/// <summary>
		/// 聚合根标识
		/// </summary>
		[Required]
		public Guid AggregateRootId { get; private set; }

		/// <summary>
		/// 版本号
		/// </summary>
		[Required]
		public int Version { get; private set; }

		/// <summary>
		/// 创建者
		/// </summary>
		[StringLength(255)]
		public string Creator { get; set; }

		/// <summary>
		/// 创建者标识
		/// </summary>
		[StringLength(255)]
		public string CreatorId { get; set; }

		/// <summary>
		/// 时间戳
		/// </summary>
		[Required]
		public DateTime Timestamp { get; set; }

		public StoredEvent()
		{
			Timestamp = DateTime.UtcNow;
		}

		public StoredEvent(Event @event) : this()
		{
			EventType = @event.GetType().AssemblyQualifiedName;
			Version = @event.Version;
			Event = Singleton<IJsonConvert>.Instance.SerializeObject(@event);
			AggregateRootId = @event.Id;
			Creator = @event.Creator;
		}

		public Event ToEvent()
		{
			return (Event) Singleton<IJsonConvert>.Instance.DeserializeObject(Event,
				Type.GetType(EventType));
		}
	}
}