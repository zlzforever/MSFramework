using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.Domain;
using MSFramework.Reflection;

namespace Ordering.API.Application.Command
{
	public class UoWInterceptor<TCommand> : ICommandInterceptor<TCommand> where TCommand : class, ICommand
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWork _unitOfWork;

		public UoWInterceptor(IUnitOfWork unitOfWork, ILogger<UoWInterceptor<TCommand>> logger)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task ExecuteAsync(TCommand command, Action<TCommand> next)
		{
			try
			{
				_logger.LogInformation("----- Handling command {CommandName} ({@Command})",
					command.GetGenericTypeName(),
					command);
				next(command);
				_logger.LogInformation("----- Command {CommandName} handled - response: {@Response}",
					command.GetGenericTypeName(), command);
			}
			finally
			{
				await _unitOfWork.CommitAsync();
			}
		}
	}
}