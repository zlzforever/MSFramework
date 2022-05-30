using System;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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

		[HttpPost]
		public M ModelValid(M m)
		{
			return m;
		}
	}

	public class M
	{
		public DateTimeOffset FullDateTimeOffset { get; set; }
		public DateTimeOffset PartDateTimeOffset { get; set; }
		public ObjectId ObjectId { get; set; }
		public int Usage { get; set; }
	}
}