using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Ordering.API.Controllers;

[Route("[controller]")]
[ApiController]
public class ValueController : ApiControllerBase
{
    [HttpGet("int")]
    public int Get([FromQuery, Required, StringLength(4, ErrorMessage = "长度不能超过 4")] string a)
    {
        return new Random().Next(0, 10);
    }

    [HttpPost]
    public M ModelValid(M m)
    {
        return m;
    }


    [Topic("pubsub", "test")]
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

    [HttpGet("noData")]
    public Task GetNoRes()
    {
        return Task.CompletedTask;
    }

    [HttpGet("list")]
    public IEnumerable<int> Get1()
    {
        return new List<int> { 1, 2, 3 };
    }

    [HttpGet("apiResult")]
    public ApiResult<List<int>> Get2()
    {
        return new ApiResult<List<int>>(new List<int> { 1, 2, 3 });
    }

    [HttpGet("file")]
    public IActionResult Get3()
    {
        var stream = System.IO.File.ReadAllBytes("1.csv");
        return new FileStreamResult(new MemoryStream(stream), "text/csv");
    }

    [HttpGet("actionResult")]
    public IActionResult Get4()
    {
        return new ObjectResult(null);
    }

    [HttpGet("pagedResult")]
    public PagedResult<int> Get5()
    {
        return new PagedResult<int>(1, 10, 10, new[] { 1, 2, 3 });
    }

    [HttpGet("emptyResult")]
    public IActionResult Get6()
    {
        return new EmptyResult();
    }
}

public class M
{
    public DateTimeOffset FullDateTimeOffset { get; set; }
    public DateTimeOffset PartDateTimeOffset { get; set; }
    public ObjectId ObjectId { get; set; }
    public int Usage { get; set; }
}
