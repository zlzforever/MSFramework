using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Initializer
{
	public abstract class InitializerBase
	{
		public virtual int Order => 100;

		public abstract Task InitializeAsync(IServiceProvider serviceProvider);

		public override string ToString()
		{
			return $"[{GetType().Name}] Order = {Order}";
		}
	}
}