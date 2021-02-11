using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;

namespace MicroserviceFramework.Application.CQRS
{
	public interface ICommandHandler<in TCommand> : IScopeDependency where TCommand : ICommand
	{
		Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
	}

	public interface ICommandHandler<in TCommand, TResponse> : IScopeDependency where TCommand : ICommand<TResponse>
	{
		Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
	}
}