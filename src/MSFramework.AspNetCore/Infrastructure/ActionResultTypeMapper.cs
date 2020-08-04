using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MSFramework.AspNetCore.Mvc;

namespace MSFramework.AspNetCore.Infrastructure
{
	public class ActionResultTypeMapper : IActionResultTypeMapper
	{
		public Type GetResultDataType(Type returnType)
		{
			if (returnType == null)
			{
				throw new ArgumentNullException(nameof(returnType));
			}
			
			return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>)
				? returnType.GetGenericArguments()[0]
				: returnType;
		}

		public IActionResult Convert(object value, Type returnType)
		{
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));
			switch (value)
			{
				case IConvertToActionResult convertToActionResult:
					return convertToActionResult.Convert();
				case IResponse _:
				{
					return new JsonResult(value);
				}
				default:
				{
					return new JsonResult(new Response(value));
				}
			}
		}
	}
}