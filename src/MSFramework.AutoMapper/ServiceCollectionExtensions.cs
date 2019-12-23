using System;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.AutoMapper
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddAutoMapper(this MSFrameworkBuilder builder,
			params Assembly[] assemblies)
		{
			builder.Services.AddScoped<MSFramework.Data.IMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(assemblies);
			return builder;
		}
		
		public static MSFrameworkBuilder AddAutoMapper(this MSFrameworkBuilder builder,
			params Type[] types)
		{
			builder.Services.AddScoped<MSFramework.Data.IMapper, AutoMapperMapper>();
			builder.Services.AddAutoMapper(types);
			return builder;
		}
	}
}