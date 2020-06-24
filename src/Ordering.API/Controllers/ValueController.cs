using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MSFramework.AspNetCore;

namespace Ordering.API.Controllers
{
	public class CreateViewObject
	{
		[Required] [StringLength(4)] public string Name { get; set; }
	}

	public class ValueController : ApiControllerBase
	{
		public ValueController()
		{
		}

		[HttpGet("getViewObject")]
		public CreateViewObject GetViewObjectAsync()
		{
			return new CreateViewObject
			{
				Name = Guid.NewGuid().ToString()
			};
		}

		[UnitOfWork]
		[HttpGet]
		public string GetAsync()
		{
			return "";
		}

		[HttpPost]
		public IActionResult CreateAsync(CreateViewObject vo)
		{
			return Ok(new
			{
				Name = vo.Name
			});
		}
	}
}