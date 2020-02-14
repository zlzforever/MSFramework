using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Extensions;

namespace MSFramework.AspNetCore.Permission.Extensions
{
	public static class ActionDescriptorExtensions
	{
		public static Permission GetPermission(this ActionDescriptor actionDescriptor)
		{
			var controllerAction = (ControllerActionDescriptor) actionDescriptor;
			var identification = actionDescriptor.GetFunctionPath();
			var parameters = controllerAction.Parameters.Select(x => $"{x.ParameterType.Name} {x.Name}")
				.ExpandAndToString();

			var permissionAttributeData =
				controllerAction.MethodInfo.CustomAttributes.FirstOrDefault(x =>
					x.AttributeType == typeof(PermissionAttribute));
			if (permissionAttributeData != null)
			{
				return new Permission
				{
					Name = permissionAttributeData.NamedArguments?.FirstOrDefault(x => x.MemberName == "Name")
						.TypedValue.Value?.ToString(),
					Module = permissionAttributeData.NamedArguments?.FirstOrDefault(x => x.MemberName == "Module")
						.TypedValue.Value?.ToString(),
					Description = permissionAttributeData.NamedArguments
						?.FirstOrDefault(x => x.MemberName == "Description")
						.TypedValue.Value?.ToString(),
					Identification = identification
				};
			}
			else
			{
				var name =
					$"{controllerAction.MethodInfo.DeclaringType?.FullName}.{controllerAction.MethodInfo.Name}({parameters})";
				return new Permission
				{
					Name = name,
					Identification = identification
				};
			}
		}
	}
}