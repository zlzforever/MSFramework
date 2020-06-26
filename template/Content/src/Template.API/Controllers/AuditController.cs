using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSFramework.AspNetCore;
using MSFramework.Audit;
using MSFramework.Domain;
using MSFramework.Ef.Repository;

#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif

namespace Template.API.Controllers
{
	[Route("api/v1.0/audit")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class AuditController :  ApiControllerBase
	{
		private readonly IRepository<AuditedOperation> _repository;

		public AuditController(IRepository<AuditedOperation> repository)
		{
			_repository = repository;
		}

		[HttpGet("GetAudits")]
		public List<AuditedOperation> GetAudits()
		{
			return ((EfRepository<AuditedOperation>) _repository).CurrentSet.Include(x => x.Entities).ToList();
		}

		[HttpGet("GetDefaultValueType")]
		public int GetDefaultValueType()
		{
			return 1;
		}
	}
}