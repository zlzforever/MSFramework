using Microsoft.AspNetCore.Builder;

namespace MSFramework.Common
{
	public abstract class Initializer
	{
		public virtual int Order => 0;

		public abstract void Initialize(IApplicationBuilder builder);
	}
}