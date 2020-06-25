using System.Collections.Generic;

namespace MSFramework.Function
{
	public interface IFunctionFinder
	{
		IEnumerable<FunctionDefine> GetAllList();
	}
}