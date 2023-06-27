using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

    [HttpGet("string")]
    public string GetString()
    {
        return Guid.NewGuid().ToString();
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

    [HttpGet("get1")]
    public IEnumerable<int> Get1()
    {
        return new List<int> { 1, 2, 3 };
    }

    [HttpGet("get2")]
    public ApiResult<List<int>> Get2()
    {
        return new ApiResult<List<int>>(new List<int> { 1, 2, 3 });
    }

    [HttpGet("get3")]
    public IActionResult Get3()
    {
        var stream = System.IO.File.ReadAllBytes("1.csv");
        return new FileStreamResult(new MemoryStream(stream), "text/csv");
    }


    [HttpGet("get4")]
    public IActionResult Get4()
    {
        return new ObjectResult(null);
    }
}

public class M
{
    public DateTimeOffset FullDateTimeOffset { get; set; }
    public DateTimeOffset PartDateTimeOffset { get; set; }
    public ObjectId ObjectId { get; set; }
    public int Usage { get; set; }
}
