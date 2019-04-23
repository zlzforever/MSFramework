using System;
using System.Threading.Tasks;

namespace MSFramework.Command
{
	public interface ICommandInterceptor<TCommand> where TCommand : class, ICommand
	{
		Task ExecuteAsync(TCommand command, Action<TCommand> next);
	}

	internal class CommandInterceptorHandler<TCommand> : ICommandHandler<TCommand> where TCommand : class, ICommand
	{
		public ICommandInterceptor<TCommand> Current { get; set; }

		public CommandInterceptorHandler<TCommand> Next { get; set; }

		public virtual async Task ExecuteAsync(TCommand command)
		{
			await Current.ExecuteAsync(command, async c =>
			{
				//
				await Next.ExecuteAsync(c);
			});
		}
	}

	internal class FinalCommandInterceptorHandler<TCommand> : CommandInterceptorHandler<TCommand>
		where TCommand : class, ICommand
	{
		private readonly ICommandHandler<TCommand> _handler;

		public FinalCommandInterceptorHandler(ICommandHandler<TCommand> handler)
		{
			_handler = handler;
		}

		public override async
			Task ExecuteAsync(TCommand command)
		{
			await _handler.ExecuteAsync(command);
		}
	}
}