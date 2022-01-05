using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;

#endif

namespace Template.API.Controllers
{
	/// <summary>
	/// FOR TEST
	/// </summary>
	[Route("api/v1.0/audit")]
	[ApiController]
	[AllowAnonymous]
	public class AuditController : ApiControllerBase
	{
		private readonly DbContextFactory _dbContextFactory;
		private readonly IAuditStore _auditStore;

		public AuditController(DbContextFactory dbContextFactory, IAuditStore auditStore)
		{
			_dbContextFactory = dbContextFactory;
			_auditStore = auditStore;
		}

		[HttpGet("GetAudits")]
		public List<AuditOperation> GetAudits()
		{
			var dbContext = _dbContextFactory.GetDbContext<AuditOperation>();
			return dbContext.Set<AuditOperation>()
				.Include(x => x.Entities).ToList();
		}

		[HttpGet("GetDefaultValueType")]
		public int GetDefaultValueType()
		{
			return 1;
		}
	}
}