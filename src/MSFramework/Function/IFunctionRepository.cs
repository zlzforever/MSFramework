using System.Collections.Generic;
using MSFramework.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.Function
{
	public interface IFunctionRepository : IRepository<FunctionDefine>, IScopeDependency
	{
		FunctionDefine GetByCode(string code);

		IEnumerable<FunctionDefine> GetAllList();
	}
}