using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Function
{
	public interface IFunctionDefineRepository : IRepository, IScopeDependency
	{
		FunctionDefine GetByCode(string code);

		IEnumerable<FunctionDefine> GetAllList();

		Task InsertAsync(FunctionDefine entity);

		Task UpdateAsync(FunctionDefine entity);

		bool IsAvailable();
	}
}