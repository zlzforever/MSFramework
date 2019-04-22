using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using MSFramework.Domain;
using MSFramework.Domain.Entity;
using Newtonsoft.Json;

namespace MSFramework.CQRS.EventSouring
{
	public class EventSourceEntry : EntityBase<long>
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

		public EventSourceEntry(IAggregateEvent @event)
		{
			EventType = @event.GetType().Name;
			Version = @event.Version;
			Event = JsonConvert.SerializeObject(@event);
			AggregateId = @event.IdAsString();
			CreationTime = DateTime.UtcNow;
		}
	}
}