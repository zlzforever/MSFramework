using System;
using System.Threading.Tasks;

namespace MSFramework.Initializer
{
	public abstract class InitializerBase
	{
		public virtual int Order => 100;

		public abstract Task InitializeAsync(IServiceProvider serviceProvider);
	}
}