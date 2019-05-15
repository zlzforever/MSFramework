using System.Collections.Generic;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IEventProvider
	{
		IReadOnlyCollection<Event> GetUncommittedChanges();

		void LoadFromHistory(params Event[] histories);
		
		void ClearChanges();
	}
}