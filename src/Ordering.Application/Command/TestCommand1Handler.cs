using System;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Application;

namespace Ordering.Application.Command
{
	public class TestCommand1Handler : ICommandHandler<TestCommand1, string>
	{
		public Task<string> HandleAsync(TestCommand1 command, CancellationToken cancellationToken)
		{
			return Task.FromResult(command.Name + new Random().Next(1, 1000));
		}
	}
}