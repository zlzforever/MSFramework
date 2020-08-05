using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Application
{
	public interface IRequestProcessor
	{
		Task ProcessAsync(IRequest request, CancellationToken cancellationToken = default);

		Task<TResult> ProcessAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
	}
}