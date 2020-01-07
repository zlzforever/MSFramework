using System.Threading.Tasks;

namespace MSFramework.EventBus
{
	public interface IDynamicEventHandler
	{
		Task Handle(dynamic @event);
	}
}