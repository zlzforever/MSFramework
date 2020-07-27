using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Application
{
	public interface ICommandHandler
	{
	}

	public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
	{
		Task HandleAsync(TCommand command, CancellationToken cancellationToken);
	}

	public interface ICommandHandler<in TCommand, TResult> : ICommandHandler where TCommand : ICommand<TResult>
	{
		Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
	}
}