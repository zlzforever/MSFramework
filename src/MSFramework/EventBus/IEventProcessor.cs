using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus;

public interface IEventProcessor
{
    Task ExecuteAsync(string eventName, string eventData);
}
