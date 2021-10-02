using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroserviceFramework
{
	public abstract class InitializerBase : IHostedService, ISingletonDependency
	{
		public abstract Task StartAsync(CancellationToken cancellationToken);
		
		public virtual Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}