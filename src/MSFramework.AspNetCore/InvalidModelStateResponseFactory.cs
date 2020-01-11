using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MSFramework.AspNetCore
{
	public static class InvalidModelStateResponseFactory
	{
		public static Func<ActionContext, IActionResult> Instance =
			context =>
			{
				var errors = context.ModelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
					.Select(x =>
						new
						{
							name = x.Key,
							error = x.Value.Errors.FirstOrDefault()?.ErrorMessage
						});

				return new ApiResult(new
				{
					success = false,
					code = 20000,
					msg = "数据校验不通过",
					errors
				});
			};
	}
}