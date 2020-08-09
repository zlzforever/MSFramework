using System.Collections.Generic;

namespace MSFramework.Functions
{
	public interface IFunctionFinder
	{
		IEnumerable<Function> GetAllList();
	}
}