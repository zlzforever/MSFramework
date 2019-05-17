using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFramework.Command
{
	public class DefaultCommandHandlerFactory : ICommandHandlerFactory
	{
		public 
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