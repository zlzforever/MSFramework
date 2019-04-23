using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandBus
	{
		Task Send<TCommand>(TCommand command) where TCommand : class, ICommand;
	}
}