using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Application.CQRS.Query
{
	public interface IQueryProcessor
	{
		Task QueryAsync(IQuery request, CancellationToken cancellationToken = default);

		Task<TResult> QueryAsync<TResult>(IQuery<TResult> request, CancellationToken cancellationToken = default);
	}
}