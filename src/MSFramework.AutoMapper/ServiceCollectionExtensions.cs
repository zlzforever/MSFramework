using System;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AutoMapper
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder)
		{
			return builder.UseAutoMapper(RuntimeUtilities.GetAllAssemblies());
		}

		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
			builder.Services.AddAutoMapper(assemblies);
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
			params Type[] types)
		{
			return builder.UseAutoMapper(types.Select(x => x.Assembly).ToArray());
		}
	}
}