using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application.CQRS;

namespace Ordering.Application.Commands
{
	public class TestCommand2Handler
		: ICommandHandler<TestCommand2>
	{
		public Task HandleAsync(TestCommand2 command, CancellationToken cancellationToken)
		{
			Console.WriteLine(command.Name);
			return Task.CompletedTask;
		}
	}
}