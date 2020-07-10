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
		private readonly IRepository<AuditOperation> _repository;

		public AuditController(IRepository<AuditOperation> repository)
		{
			_repository = repository;
		}

		[HttpGet("GetAudits")]
		public List<AuditOperation> GetAudits()
		{
			return ((EfRepository<AuditOperation>) _repository).CurrentSet.Include(x => x.Entities).ToList();
		}

		[HttpGet("GetDefaultValueType")]
		public int GetDefaultValueType()
		{
			return 1;
		}
	}
}