using System.Collections.Generic;

namespace MSFramework.Functions
{
	public interface IFunctionFinder
	{
		IEnumerable<FunctionDefine> GetAllList();
	}
}