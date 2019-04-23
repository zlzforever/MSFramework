using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Domain.Entity;
using MSFramework.Serialization;

namespace MSFramework.EventSouring
{
	public class EventHistory : EntityBase<long>
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(255)]
		public string EventType { get; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public string Event { get; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string AggregateId { get; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public long Version { get; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime CreationTime { get; }

		public EventHistory(IAggregateEvent @event)
		{
			EventType = @event.GetType().Name;
			Version = @event.Version;
			Event = Singleton<IJsonConvert>.Instance.SerializeObject(@event);
			AggregateId = @event.IdAsString();
			CreationTime = DateTime.UtcNow;
		}

		public IAggregateEvent ToAggregateEvent()
		{
			return (IAggregateEvent) Singleton<IJsonConvert>.Instance.DeserializeObject(Event, Type.GetType(EventType));
		}
	}
}