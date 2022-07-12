using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroserviceFramework.AspNetCore.Swagger;

public class ObjectIdOperationFilter : IOperationFilter
{
	//prop names we want to ignore
	private readonly IEnumerable<string> _objectIdIgnoreParameters = new[]
	{
		"Timestamp",
		"Machine",
		"Pid",
		"Increment",
		"CreationTime"
	};

	private readonly IEnumerable<XPathNavigator> _xmlNavigators;

	public ObjectIdOperationFilter(IEnumerable<string> filePaths)
	{
		_xmlNavigators = filePaths != null
			? filePaths.Select(x => new XPathDocument(x).CreateNavigator())
			: Array.Empty<XPathNavigator>();
	}

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		//for very parameter in operation check if any fields we want to ignore
		//delete them and add ObjectId parameter instead
		foreach (var p in operation.Parameters.ToList())
			if (_objectIdIgnoreParameters.Any(x => p.Name.EndsWith(x)))
			{
				var parameterIndex = operation.Parameters.IndexOf(p);
				operation.Parameters.Remove(p);
				var dotIndex = p.Name.LastIndexOf(".", StringComparison.Ordinal);
				if (dotIndex > -1)
				{
					var idName = p.Name.Substring(0, dotIndex);
					if (operation.Parameters.All(x => x.Name != idName))
					{
						operation.Parameters.Insert(parameterIndex, new OpenApiParameter()
						{
							Name = idName,
							Schema = new OpenApiSchema()
							{
								Type = "string",
								Format = "24-digit hex string"
							},
							Description = GetFieldDescription(idName, context),
							Example = new OpenApiString(ObjectId.Empty.ToString()),
							In = p.In,
						});
					}
				}
			}
	}

	//get description from XML
	private string GetFieldDescription(string idName, OperationFilterContext context)
	{
		var name = char.ToUpperInvariant(idName[0]) + idName.Substring(1);
		var classProp = context.MethodInfo.GetParameters().FirstOrDefault()?.ParameterType.GetProperties()
			.FirstOrDefault(x => x.Name == name);
		var typeAttr = classProp != null
			? classProp.GetCustomAttribute<DescriptionAttribute>()
			: null;
		if (typeAttr != null)
		{
			return typeAttr.Description;
		}

		if (classProp == null)
		{
			return null;
		}

		foreach (var xmlNavigator in _xmlNavigators)
		{
			var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(classProp);
			var propertySummaryNode =
				xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
			if (propertySummaryNode != null)
			{
				return XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml);
			}
		}

		return null;
	}
}