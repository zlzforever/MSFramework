using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroserviceFramework.AspNetCore.Swagger;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static SwaggerGenOptions MapEnumerationType(this SwaggerGenOptions options, Assembly assembly)
    {
        var enumTypes = assembly.GetTypes().Where(i => i.IsSubclassOf(typeof(Enumeration)));
        foreach (var enumType in enumTypes)
        {
            var enumDoc = new List<IOpenApiAny>();
            var enums = GetAll(enumType);
            foreach (var enumeration in enums)
            {
                enumDoc.Add(new OpenApiString(enumeration.Id));
            }

            options.MapType(enumType, () => new OpenApiSchema { Type = "enum", Enum = enumDoc });
        }

        return options;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static SwaggerGenOptions SupportObjectId(this SwaggerGenOptions options)
    {
        options.SchemaFilter<ObjectIdSchemaFilter>();
        options.OperationFilter<ObjectIdOperationFilter>();
        return options;
    }

    private static IEnumerable<Enumeration> GetAll(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(i => i.FieldType.IsSubclassOf(typeof(Enumeration))).Select(f => (Enumeration)f.GetValue(null));
    }
}
