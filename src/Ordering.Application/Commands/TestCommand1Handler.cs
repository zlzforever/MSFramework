using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands
{
	public class TestCommand1Handler : IRequestHandler<TestCommand1, string>
	{
		public Task<string> HandleAsync(TestCommand1 command, CancellationToken cancellationToken)
		{
			return Task.FromResult(command.Name + new Random().Next(1, 1000));
		}
	}
}