using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Domain;
using MSFramework.Http;

namespace Ordering.API.Controllers
{
	public class CreateViewObject
	{
		[Required] [StringLength(4)] public string Name { get; set; }
	}

	public class ValueController : MSFrameworkApiControllerBase
	{
		private TestService _testService;
		private ApiClient _apiClient;

		public ValueController(TestService testService, ApiClient apiClient, IMSFrameworkSession session,
			ILogger<ValueController> logger) : base(session, logger)
		{
			_testService = testService;
			_apiClient = apiClient;
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
		public async Task<object> GetAsync()
		{
			var host = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
			var apiResponse =
				await _apiClient.GetAsync<CreateViewObject>(
					"default", $"{host}/getViewObject");
			return new
			{
				apiResponse.Data.Name
			};
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