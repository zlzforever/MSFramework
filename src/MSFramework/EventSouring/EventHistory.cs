using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Domain.Event;
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
		public string EventType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public string Event { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string AggregateId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public long Version { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime CreationTime { get; set; }

		public EventHistory()
		{
		}

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