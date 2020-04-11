using System;
using System.Threading.Tasks;

namespace MSFramework.Common
{
	public abstract class Initializer
	{
		public virtual int Order => 0;

		public abstract Task InitializeAsync(IServiceProvider serviceProvider);
	}
}