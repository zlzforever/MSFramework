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

		public async Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand
		{
			var handler = _commandHandlerFactory.GetHandler<TCommand>();
			if (handler != null)
			{
				await handler.HandleAsync(command);
			}
			else
			{
				throw new MSFrameworkException("no handler registered");
			}
		}
	}
}