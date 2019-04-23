using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.Reflection;

namespace Ordering.API.Application.Command
{
	public class LogInterceptor<TCommand> : ICommandInterceptor<TCommand> where TCommand : class, ICommand
	{
		private readonly ILogger _logger;

		public LogInterceptor(ILogger<LogInterceptor<TCommand>> logger)
		{
			_logger = logger;
		}

		public Task ExecuteAsync(TCommand command, Action<TCommand> next)
		{
			_logger.LogInformation("----- Handling command {CommandName} ({@Command})", command.GetGenericTypeName(),
				command);
			next(command);
			_logger.LogInformation("----- Command {CommandName} handled - response: {@Response}",
				command.GetGenericTypeName(), command);
			return Task.CompletedTask;
		}
	}
}