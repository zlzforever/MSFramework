using System;
using System.Collections.Generic;

namespace MSFramework.Function
{
	public interface IFunctionStore
	{
		FunctionDefine Get(string path);

		List<FunctionDefine> GetAllList();

		void Update(FunctionDefine function);

		void Delete(Guid id);

		void Add(FunctionDefine function);
	}
}