using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregateRoots;
using Ordering.Infrastructure;

namespace Ordering.API.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController(OrderingContext orderingContext) : ApiControllerBase
{
    [HttpGet("all")]
    public List<UserInfo> GetAllListAsync()
    {
        return orderingContext.Set<UserInfo>()
            .AsNoTracking().ToList();
    }
}
