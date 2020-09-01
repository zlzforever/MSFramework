using System.Collections.Generic;

namespace MicroserviceFramework.Functions
{
	public interface IFunctionFinder
	{
		IEnumerable<Function> GetAllList();
	}
}