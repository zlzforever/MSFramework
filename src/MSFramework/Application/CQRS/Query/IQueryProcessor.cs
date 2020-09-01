using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS.Query
{
	public interface IQueryProcessor
	{
		Task ProcessAsync(IQuery request, CancellationToken cancellationToken = default);

		Task<TResult> ProcessAsync<TResult>(IQuery<TResult> request, CancellationToken cancellationToken = default);
	}
}