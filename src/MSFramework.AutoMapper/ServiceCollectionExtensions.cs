using System;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using IMapper = MSFramework.Mapper.IMapper;

namespace MSFramework.AutoMapper
{
	public static class ServiceCollectionExtensions
	{
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