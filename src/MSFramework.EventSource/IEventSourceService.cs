using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MSFramework.EventBus;

namespace MSFramework.EventSource
{
	public interface IEventSourceService
	{ 		
		Task PublishEventsAsync();
		Task AddEventAsync(Event @event);
	}
}