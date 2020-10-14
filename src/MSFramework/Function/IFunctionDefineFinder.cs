using System.Collections.Generic;

namespace MicroserviceFramework.Function
{
	public interface IFunctionDefineFinder
	{
		IEnumerable<FunctionDefine> GetAllList();
	}
}