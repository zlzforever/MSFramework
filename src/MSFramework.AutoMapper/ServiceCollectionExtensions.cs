using System;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Reflection;
using IMapper = MSFramework.Mapper.IMapper;

namespace MSFramework.AutoMapper
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseAutoMapper(this MSFrameworkBuilder builder)
		{
			var assemblies = AssemblyFinder.GetAllList();
			return builder.UseAutoMapper(assemblies.ToArray());
		}

		public static MSFrameworkBuilder UseAutoMapper(this MSFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddScoped<IMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(assemblies);
			return builder;
		}

		public static MSFrameworkBuilder UseAutoMapper(this MSFrameworkBuilder builder,
			params Type[] types)
		{
			builder.Services.AddScoped<IMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(types);
			return builder;
		}
	}
}