using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public interface IEventExecutor
{
	Task ExecuteAsync(string eventName, string @event);
}