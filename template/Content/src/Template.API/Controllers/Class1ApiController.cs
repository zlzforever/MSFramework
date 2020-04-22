using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
using MSFramework.AspNetCore.Permission;
#endif
using MSFramework.Data;
using MSFramework.Domain;
using Template.API.ViewObject;
using Template.Application.DTO;
using Template.Application.Query;
using Template.Application.Service;

namespace Template.API.Controllers
{
	[Route("api/v1.0/class1")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class Class1ApiController : MSFrameworkApiControllerBase
	{
		private readonly IClass1Query _class1Query;
		private readonly IClass1Service _class1Service;
		private readonly IMapper _mapper;

		public Class1ApiController(
			IClass1Query class1Query,
			IClass1Service class1Service,
			IMapper mapper,
			IMSFrameworkSession session, ILogger<Class1ApiController> logger) : base(session, logger)
		{
			_class1Query = class1Query;
			_class1Service = class1Service;
			_mapper = mapper;
		}

		[HttpGet]
#if !DEBUG
		[Permission(Name = "查询 Class1", Module = "Class1")]
#endif
		public async Task<PagedQueryResult<Class1Out>> PagedQueryAsync(string keyword, int page, int limit)
		{
			return await _class1Query.PagedQueryAsync(keyword, page, limit);
		}

		[HttpPost]
#if !DEBUG
		[Permission(Name = "创建 Class1", Module = "Class1")]
#endif
		public async Task<CreatClass1Out> CreateAsync([FromBody] CreateClass1ViewObject vo)
		{
			var class1 = _mapper.Map<CreateClass1In>(vo);
			var result = await _class1Service.CreateAsync(class1);
			return result;
		}
	}
}