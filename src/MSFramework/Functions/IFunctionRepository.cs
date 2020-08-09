using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Functions
{
	public interface IFunctionRepository : IRepository, IScopeDependency
	{
		Function GetByCode(string code);

		IEnumerable<Function> GetAllList();

		Task InsertAsync(Function entity);

		Task UpdateAsync(Function entity);

		bool IsAvailable();
	}
}