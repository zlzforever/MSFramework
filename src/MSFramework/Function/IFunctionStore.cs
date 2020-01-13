using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFramework.Function
{
	public interface IFunctionStore
	{
		Function Get(string path);

		List<Function> GetAllList();

		void Update(Function function);

		void Delete(Guid id);

		void Add(Function function);
	}
}