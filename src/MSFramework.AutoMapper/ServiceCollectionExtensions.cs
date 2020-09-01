using System;
using System.Reflection;
using AutoMapper;
using MicroserviceFramework.Mapper;
using MicroserviceFramework.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AutoMapper
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder)
		{
			var assemblies = AssemblyFinder.GetAllList();
			return builder.UseAutoMapper(assemblies.ToArray());
		}

		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddScoped<IObjMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(assemblies);
			return builder;
		}

		public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
			params Type[] types)
		{
			builder.Services.AddScoped<IObjMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(types);
			return builder;
		}
	}
}