using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Functions
{
	public interface IFunctionRepository : IRepository, IScopeDependency
	{
		FunctionDefine GetByCode(string code);

		IEnumerable<FunctionDefine> GetAllList();

		Task InsertAsync(FunctionDefine entity);

		Task UpdateAsync(FunctionDefine entity);

		bool IsAvailable();
	}
}