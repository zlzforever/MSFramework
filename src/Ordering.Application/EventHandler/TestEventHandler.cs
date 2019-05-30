using System.Threading.Tasks;
using MSFramework.EventBus;

namespace Ordering.Application.EventHandler
{
	[SubscribeName("test")]
	public class TestEventHandler : IDynamicEventHandler
	{
		public Task Handle(dynamic @event)
		{
			return Task.CompletedTask;
		}
	}
}