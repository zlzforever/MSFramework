using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Common
{
	public abstract class Initializer : IDisposable
	{
		private readonly IServiceScope _scope;
		protected IServiceProvider Services => _scope.ServiceProvider;

		protected Initializer(IServiceProvider serviceProvider)
		{
			_scope = serviceProvider.CreateScope();
		}

		public abstract void Initialize();

		public void Dispose()
		{
			_scope?.Dispose();
		}
	}
}