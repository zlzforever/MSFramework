using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandBus
	{
		Task<bool> SendAsync<TCommand>(TCommand command) where TCommand : class, ICommand;
	}
}