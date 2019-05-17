using System;
using System.Collections.Generic;

namespace MSFramework.Command
{
	public interface ICommandHandlerFactory
	{
		object GetHandler(Type commandType);

		Dictionary<Type, Type> GetHandlers();
	}
}