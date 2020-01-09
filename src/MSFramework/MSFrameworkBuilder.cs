using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework
{
	[assembly: InternalsVisibleTo("MSFramework.Ef")]
	public class MSFrameworkBuilder
	{
		public MSFrameworkBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}