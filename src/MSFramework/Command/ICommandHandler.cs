using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandHandler<in TCommand> where TCommand : ICommand
	{
		Task HandleAsync(TCommand message);
	}

	public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand
	{
		Task<TResponse> HandleAsync(TCommand message);
	}
}