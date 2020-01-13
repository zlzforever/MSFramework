using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Domain;

namespace Ordering.API.Controllers
{
	public class CreateViewObject
	{
		[Required] [StringLength(4)] public string Name { get; set; }
	}

	public class ValueController : Controller
	{
		private TestService _testService;

		public ValueController(TestService testService, IMSFrameworkSession session, ILogger<ValueController> logger)
		{
			_testService = testService;
		}

		[UnitOfWork]
		[HttpGet]
		public IActionResult GetAsync(string name)
		{
			return Ok(new
			{
				Name = _testService.Get(name)
			});
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