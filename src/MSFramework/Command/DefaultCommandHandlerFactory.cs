using System;
using System.Collections.Generic;

namespace MSFramework.Command
{
	public class DefaultCommandHandlerFactory : ICommandHandlerFactory
	{
		  
		public object GetHandler(Type commandType)
		{
			throw new NotImplementedException();
		}

		public Dictionary<Type, Type> GetHandlers()
		{
			throw new NotImplementedException();
		}
	}
}