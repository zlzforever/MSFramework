using System;
using System.Threading.Tasks;

namespace MSFramework.Initializers
{
	public abstract class Initializer
	{
		public virtual int Order => 100;

		public abstract Task InitializeAsync(IServiceProvider serviceProvider);

		public override string ToString()
		{
			return $"[{GetType().Name}] Order = {Order}";
		}
	}
}