using System;

namespace MSFramework.Domain
{
	public class EventSourceEntry<TAggregateId> where TAggregateId : IEquatable<TAggregateId>
	{
		public string EventType { get; set; }

		public string Event { get; set; }

		public TAggregateId AggregateId { get; set; }

		public long Version { get; set; }
	}
}