using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS
{
	public interface ICommandHandler<in TCommand> where TCommand : ICommand
	{
		Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
	}

	public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
	{
		Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
	}
}