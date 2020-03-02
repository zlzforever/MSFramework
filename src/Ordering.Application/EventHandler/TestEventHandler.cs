using System.Threading.Tasks;
using EventBus;

namespace Ordering.Application.EventHandler
{
	public class TestEventHandler : IDynamicEventHandler
	{
		public Task HandleAsync(object @event)
		{
			throw new System.NotImplementedException();
		}
	}
}