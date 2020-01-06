using System;

namespace MSFramework
{
	public class MSFrameworkApplicationBuilder : IMSFrameworkApplicationBuilder
	{
		public IServiceProvider ApplicationServices { get; private set; }

		public MSFrameworkApplicationBuilder(IServiceProvider applicationServices)
		{
			ApplicationServices = applicationServices;
		}
	}
}