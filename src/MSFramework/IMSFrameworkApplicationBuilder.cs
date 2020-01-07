using System;

namespace MSFramework
{
	public interface IMSFrameworkApplicationBuilder
	{
		IServiceProvider ApplicationServices { get; }
	}
}