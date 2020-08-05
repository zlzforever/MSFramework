using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Application;
using Xunit;

namespace MSFramework.Tests
{
	public class Request1 : IRequest, IRequestHandler<Request1>
	{
		public Task HandleAsync(Request1 request, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}

	public class Request2 : IRequest<int>, IRequestHandler<Request2, int>
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
			services.AddRequestProcessor(GetType().Assembly);
			var provider = services.BuildServiceProvider();
			var processor = provider.GetRequiredService<IRequestProcessor>();
			await processor.ProcessAsync(new Request1());
			var result = await processor.ProcessAsync(new Request2());
			Assert.Equal(1, result);
		}
	}
}