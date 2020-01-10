using System.Threading.Tasks;
using MSFramework.Data;
using MSFramework.DependencyInjection;
using Template.Application.DTO;

namespace Template.Application.Query
{
	public interface IClass1Query : IScopeDependency
	{
		Task<Class1Out> GetClass1ByNameAsync(string name);

		Task<PagedQueryResult<Class1Out>> PagedQueryAsync(string keyword, int page, int limit);
	}
}