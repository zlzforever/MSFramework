using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS.Command
{
	public interface ICommandProcessor
	{
		Task ExecuteAsync(ICommand request, CancellationToken cancellationToken = default);

		Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> request, CancellationToken cancellationToken = default);
	}
}