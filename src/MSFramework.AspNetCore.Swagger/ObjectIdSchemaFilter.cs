using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroserviceFramework.AspNetCore.Swagger;

public class ObjectIdSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ObjectId))
        {
            schema.Type = "string";
            schema.Format = "24-digit hex string";
            schema.Example = new OpenApiString(ObjectId.Empty.ToString());
        }
    }
}