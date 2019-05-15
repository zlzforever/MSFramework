using System;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseEntityFrameworkEventSouring(this MSFrameworkBuilder builder )
		{
			builder.Services.AddScoped<IEventStore, EfEventStore>();
			return builder;
		}
	}
}