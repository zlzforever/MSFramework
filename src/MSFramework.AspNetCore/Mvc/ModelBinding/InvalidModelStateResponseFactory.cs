using System;
using System.Linq;
using MicroserviceFramework.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding
{
	public static class InvalidModelStateResponseFactory
	{
		public static readonly Func<ActionContext, IActionResult> Instance = context =>
		{
			var errors = context.ModelState.Where(x =>
					x.Value is { ValidationState: ModelValidationState.Invalid })
				.ToDictionary(
					x => StringUtilities.ToCamelCase(x.Key),
					x =>
						x.Value?.Errors.Where(z => !string.IsNullOrWhiteSpace(z.ErrorMessage))
							.Select(y => y.ErrorMessage));
			return new ApiResult(null)
			{
				Code = 1,
				Success = false,
				Errors = errors,
				Msg = "数据校验不通过",
				StatusCode = 200
			};
		};
	}
}