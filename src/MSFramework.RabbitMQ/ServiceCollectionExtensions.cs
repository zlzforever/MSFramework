// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace MicroserviceFramework.RabbitMQ
// {
// 	public static class ServiceCollectionExtensions
// 	{
// 		public static MicroserviceFrameworkBuilder UseRabbitMqEventDispatcher(this MicroserviceFrameworkBuilder builder,
// 			RabbitMqOptions options, params Type[] eventTypes)
// 		{
// 			var excludeAssembly = typeof(MicroserviceFrameworkBuilder).Assembly;
// 			if (eventTypes.Any(x => x.Assembly != excludeAssembly))
// 			{
// 				var list = new List<Type>(eventTypes) {typeof(MicroserviceFrameworkBuilder)};
// 				builder.Services.AddEventDispatcher(list.ToArray());
// 			}
// 			else
// 			{
// 				builder.Services.AddEventDispatcher(eventTypes);
// 			}
//
// 			builder.Services.AddSingleton(options);
// 			builder.Services.AddSingleton<IEventDispatcher, RabbitMqEventDispatcher>();
// 			return builder;
// 		}
//
// 		public static MicroserviceFrameworkBuilder UseRabbitMqEventDispatcher(this MicroserviceFrameworkBuilder builder,
// 			Action<RabbitMqOptions> configure, params Type[] eventTypes)
// 		{
// 			var options = new RabbitMqOptions();
// 			configure?.Invoke(options);
//
// 			builder.UseRabbitMqEventDispatcher(options, eventTypes);
// 			return builder;
// 		}
// 	}
// }