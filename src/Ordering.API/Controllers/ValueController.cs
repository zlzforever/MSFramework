using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Ordering.API.Controllers;

[Route("[controller]")]
[ApiController]
public class ValueController : ApiControllerBase
{
    [HttpGet]
    public int Get([FromQuery, Required, StringLength(4, ErrorMessage = "长度不能超过 4")] string a)
    {
        return new Random().Next(0, 10);
    }

    [HttpPost]
    public M ModelValid(M m)
    {
        
        return m;
    }

    [HttpGet("ok")]
    public ApiResult GetOk()
    {
        return ApiResult.Ok;
    }

    [HttpGet("error")]
    public ApiResult GetError()
    {
        return ApiResult.Error;
    }

    [HttpGet("nores")]
    public Task GetNoRes()
    {
        return Task.CompletedTask;
    }
}

public class M
{
    public DateTimeOffset FullDateTimeOffset { get; set; }
    public DateTimeOffset PartDateTimeOffset { get; set; }
    public ObjectId ObjectId { get; set; }
    public int Usage { get; set; }
}
