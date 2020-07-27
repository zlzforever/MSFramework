using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Application
{
	public interface IRequestHandler
	{
	}

	public interface IRequestHandler<in TCommand> : IRequestHandler where TCommand : IRequest
	{
		Task HandleAsync(TCommand command, CancellationToken cancellationToken);
	}

	public interface IRequestHandler<in TCommand, TResult> : IRequestHandler where TCommand : IRequest<TResult>
	{
		Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
	}
}