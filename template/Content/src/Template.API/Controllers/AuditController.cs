using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
	public class AuditController : ApiControllerBase
	{
		private readonly IRepository<AuditOperation> _repository;

		public AuditController(IRepository<AuditOperation> repository)
		{
			_repository = repository;
		}

		[HttpGet("GetAudits")]
		public List<AuditOperation> GetAudits()
		{
			return ((EfRepository<AuditOperation>) _repository).AggregateRootSet
				.Include(x => x.Entities).ToList();
		}

		[HttpGet("GetDefaultValueType")]
		public int GetDefaultValueType()
		{
			return 1;
		}
	}
}