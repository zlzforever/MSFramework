using System.Reflection;
using System.Threading.Tasks;

namespace MSFramework.Command
{
	public class CommandBus : ICommandBus
	{
		private readonly ICommandHandlerFactory _commandHandlerFactory;

		public CommandBus(ICommandHandlerFactory commandHandlerFactory)
		{
			_commandHandlerFactory = commandHandlerFactory;
		}

		public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command)
		{
			var handler = _commandHandlerFactory.GetHandler(command.GetType());
			if (handler != null)
			{
				var method = handler.GetType().GetMethod("HandleAsync", BindingFlags.Public);
				if (method != null)
				{
					var response = (TResponse) method.Invoke(handler, new object[] {command});
					return Task.FromResult(response);
				}
			}
			else
			{
				throw new MSFrameworkException("no handler registered");
			}
		}
	}
}