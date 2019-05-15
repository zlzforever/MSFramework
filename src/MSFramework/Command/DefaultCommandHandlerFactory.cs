using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFramework.Command
{
	public class DefaultCommandHandlerFactory : ICommandHandlerFactory
	{
		public ICommandHandler<T> GetHandler<T>() where T : ICommand
		{
			return null;
		}

		public Dictionary<Type, Type> GetHandlers()
		{
			throw new NotImplementedException();
		}

		private IEnumerable<Type> GetHandlerTypes<T>() where T : ICommand
		{
			var handlers = typeof(ICommandHandler<>).Assembly.GetExportedTypes()
				.Where(x => x.GetInterfaces()
					.Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ))
				.Where(h=>h.GetInterfaces()
					.Any(ii=>ii.GetGenericArguments()
						.Any(aa=>aa==typeof(T)))).ToList();

           
			return handlers;
		}
	}
}