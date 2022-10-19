// using System;
// using System.Reflection;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Infrastructure;
//
// namespace MicroserviceFramework.AspNetCore.Mvc;
//
// public static class JsonResultHelper
// {
//     public static readonly Type ApiResultType;
//     public static readonly Type ActionResultExecutorType;
//     public static readonly MethodInfo ExecuteMethodInfo;
//
//     static JsonResultHelper()
//     {
//         ApiResultType = Type.GetType("Microsoft.AspNetCore.Mvc.JsonResult, Microsoft.AspNetCore.Mvc.Core");
//         if (ApiResultType == null)
//         {
//             throw new MicroserviceFrameworkException("未找到 ApiResult 类型");
//         }
//
//         ActionResultExecutorType = typeof(IActionResultExecutor<>).MakeGenericType(ApiResultType);
//         ExecuteMethodInfo =
//             ActionResultExecutorType.GetMethod("ExecuteAsync", new[] { typeof(ActionContext), ApiResultType });
//     }
// }
