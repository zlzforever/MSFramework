using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Functions
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