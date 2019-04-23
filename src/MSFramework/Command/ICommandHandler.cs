using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
	{
		Task ExecuteAsync(TCommand command);
	}		
}