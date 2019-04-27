using System.Threading.Tasks;

namespace MSFramework.IntegrateService
{
	public interface IIntegrateService
	{
		Task PublishIntegrateEventAsync(IntegrationEvent @event);
	}
}