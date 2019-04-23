using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.Domain;
using MSFramework.Reflection;

namespace Ordering.API.Application.Command
{
	public class TransactionInterceptor<TCommand> : ICommandInterceptor<TCommand> where TCommand : class, ICommand
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger _logger;

		public TransactionInterceptor(IUnitOfWork unitOfWork,
			ILogger<TransactionInterceptor<TCommand>> logger)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task ExecuteAsync(TCommand command, Action<TCommand> next)
		{
			try
			{
				await _unitOfWork.BeginOrUseTransactionAsync();
				next(command);
			}
			catch (Exception ex)
			{
				var typeName = command.GetGenericTypeName();
				_logger.LogError(ex, "ERROR Handling transaction for {CommandName} ({@Command})", typeName, command);

				throw;
			}
			finally
			{
				await _unitOfWork.CommitAsync();
			}
		}
	}
}