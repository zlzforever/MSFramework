using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Data;
using MSFramework.Domain;
using Template.API.ViewObject;
using Template.Application.DTO;
using Template.Application.Query;
using Template.Application.Service;
using Template.Domain;
using Template.Domain.AggregateRoot;

namespace Template.API.Controllers
{
	[Route("api/v1.0/class1")]
	[ApiController]
	[Authorize]
	public class Class1ApiController : MSFrameworkApiControllerBase
	{
		private readonly IClass1Query _class1Query;
		private readonly IClass1Service _class1Service;
		private readonly IMapper _mapper;

		public Class1ApiController(
			IClass1Query class1Query,
			IClass1Service class1Service,
			IMapper mapper,
			IMSFrameworkSession session, ILogger logger) : base(session, logger)
		{
			_class1Query = class1Query;
			_class1Service = class1Service;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> PagedQueryAsync(string keyword, int page, int limit)
		{
			var result = await _class1Query.PagedQueryAsync(keyword, page, limit);
			return PagedResult(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateAsync([FromBody] CreateClass1ViewObject vo)
		{
			var class1 = _mapper.Map<CreateClass1In>(vo);
			var result = await _class1Service.CreateAsync(class1);
			return Ok(result);
		}
	}
}