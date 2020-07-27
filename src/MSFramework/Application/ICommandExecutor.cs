using System;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Extensions;

namespace MSFramework.Application
{
	public interface ICommandExecutor
	{
		void Register(Type commandType, Type handlerType);

		Task ExecuteAsync(ICommand command, CancellationToken cancellationToken);

		Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken);
	}
}