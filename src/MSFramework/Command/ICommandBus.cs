using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandBus
	{
		Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command);
	}
}