using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Application.CQRS;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests
{
	public class Request1 : ICommand, ICommandHandler<Request1>
	{
		public Task HandleAsync(Request1 request, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

	public class Request2 : ICommand<int>, ICommandHandler<Request2, int>
	{
		public Task<int> HandleAsync(Request2 request, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(1);
		}
	}

	public class RequestProcessorTests
	{
		[Fact]
		public async Task Test1()
		{
			var services = new ServiceCollection();
			services.AddCqrs();
			var provider = services.BuildServiceProvider();
			var processor = provider.GetRequiredService<ICqrsProcessor>();
			await processor.ExecuteAsync(new Request1());
			var result = await processor.ExecuteAsync(new Request2());
			Assert.Equal(1, result);
		}
	}
}