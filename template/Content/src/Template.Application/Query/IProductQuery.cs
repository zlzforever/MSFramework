using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using Template.Application.DTO;

namespace Template.Application.Query
{
	public interface IProductQuery : IScopeDependency
	{
		Task<ProductOut> GetByNameAsync(string name);

		Task<PagedResult<ProductOut>> PagedQueryAsync(string keyword, int page, int limit);
	}
}