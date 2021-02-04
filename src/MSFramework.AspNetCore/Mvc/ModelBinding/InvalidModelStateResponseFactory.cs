using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MicroserviceFramework.AspNetCore.Mvc.ModelBinding
{
	public static class InvalidModelStateResponseFactory
	{
		public static readonly Func<ActionContext, IActionResult> Instance = context =>
		{
			var errors = context.ModelState.Where(x =>
					x.Value.ValidationState == ModelValidationState.Invalid)
				.Select(x =>
					new
					{
						name = x.Key,
						messages = x.Value.Errors.Where(z => !string.IsNullOrWhiteSpace(z.ErrorMessage))
							.Select(y => y.ErrorMessage)
					});

			return new ApiResult(new
			{
				success = false,
				code = 1,
				msg = "数据校验不通过",
				errors
			})
			{
				StatusCode = 200
			};
		};
	}
}