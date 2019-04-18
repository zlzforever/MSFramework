using System;
using System.Collections.Generic;
using System.Text;

namespace MSFramework.Domain.Event
{
	public interface IDomainEvent
	{
		object EventData { get; }
	}

	public class DomainEvent : IDomainEvent
	{
		public object EventData { get; set; }
	}
}
