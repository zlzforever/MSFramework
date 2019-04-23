using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Command
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddCommandInterceptor(this MSFrameworkBuilder builder,
			params Type[] types)
		{
			//TODO: check type is  ICommandInterceptor
			foreach (var type in types)
			{
				builder.Services.AddScoped(typeof(ICommandInterceptor<>), type);
			}

			return builder;
		}

		public static MSFrameworkBuilder AddCommandHandlers<TImplementation>(this MSFrameworkBuilder builder,
			params Type[] types)
		{
			var implType = typeof(TImplementation);
			foreach (var type in types)
			{
				if (type.IsAssignableFrom(implType))
				{
					builder.Services.AddScoped(type, implType);
				}
				else
				{
					throw new MSFrameworkException("AddCommandHandler");
				}
			}

			return builder;
		}
	}
}