using System;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.API.Controllers
{
	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class ValueController : ApiControllerBase
	{
		[HttpGet]
		public int Get()
		{
			return new Random().Next(0, 10);
		}
	}
}