using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroserviceFramework.AspNetCore.Infrastructure
{
	public static class SwaggerGenExtension
	{
		public static SwaggerGenOptions AddEnumerationDoc(this SwaggerGenOptions options, Assembly assembly)
		{
			var enumTypes = assembly.GetTypes().Where(i => i.IsSubclassOf(typeof(Enumeration)));
			foreach (var enumType in enumTypes)
			{
				var enumDoc = new List<IOpenApiAny> { };
				var enums = GetAll(enumType);
				foreach (var enumeration in enums)
				{
					enumDoc.Add(new OpenApiString(enumeration.Id));
				}

				options.MapType(enumType, () => new OpenApiSchema {Type = "enum", Enum = enumDoc});
			}

			return options;
		}

		public static SwaggerGenOptions AddObjectIdDoc(this SwaggerGenOptions options)
		{
			options.MapType<ObjectId>(() => new OpenApiSchema {Type = "string"});
			return options;
		}

		private static IEnumerable<Enumeration> GetAll(Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration))).Select(f => (Enumeration) f.GetValue(null));
		}
	}
}