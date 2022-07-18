using System;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MicroserviceFramework.AspNetCore.Infrastructure;

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
        {
            throw new ArgumentNullException(nameof(returnType));
        }

        return value switch
        {
            IConvertToActionResult convertToActionResult => convertToActionResult.Convert(),
            _ => new ApiResult(value)
        };
    }
}
