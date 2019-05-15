using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandBus
	{
		Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand;
	}
}