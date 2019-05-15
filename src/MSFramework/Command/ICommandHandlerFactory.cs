using System;
using System.Collections.Generic;

namespace MSFramework.Command
{
	public interface ICommandHandlerFactory
	{
		ICommandHandler<T> GetHandler<T>() where T : ICommand;

		Dictionary<Type, Type> GetHandlers();
	}
}