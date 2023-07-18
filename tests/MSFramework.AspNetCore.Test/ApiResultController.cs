﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore.Test;

[Route("[controller]")]
[ApiController]
public class ApiResultController : ApiControllerBase
{
    [HttpGet("int")]
    public int Get()
    {
        return 7896;
    }

    [HttpGet("string")]
    public string GetString()
    {
        return "AAABBB";
    }

    [HttpGet("dateTime")]
    public DateTime GetDateTime()
    {
        return new DateTime(2023, 07, 13, 23,
            26, 0);
    }

    [HttpGet("nullableDateTime1")]
    public DateTime? GetNullableDateTime()
    {
        return null;
    }

    [HttpGet("nullableDateTime2")]
    public DateTime? GetNullableDateTime2()
    {
        return new DateTime(2023, 07, 13, 23,
            26, 0);
    }

    [HttpGet("dateTimeOffset")]
    public DateTimeOffset GetDateTimeOffset()
    {
        return new DateTimeOffset(2023, 07, 13, 23,
            26, 0, DateTimeOffset.Now.Offset);
    }

    [HttpGet("nullableDateTimeOffset1")]
    public DateTimeOffset? GetNullableDateTimeOffset()
    {
        return null;
    }

    [HttpGet("nullableDateTimeOffset2")]
    public DateTimeOffset? GetNullableDateTimeOffset2()
    {
        return new DateTimeOffset(2023, 07, 13, 23,
            26, 0, DateTimeOffset.Now.Offset);
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

    [HttpGet("noResponse")]
    public Task GetNoRes()
    {
        return Task.CompletedTask;
    }

    [HttpGet("list1")]
    public IEnumerable<int> GetList1()
    {
        return new List<int> { 1, 2 };
    }

    [HttpGet("list2")]
    public ApiResult<List<int>> GetList2()
    {
        return new ApiResult<List<int>>(new List<int> { 1, 2, 3 });
    }

    [HttpGet("file")]
    public IActionResult GetFile()
    {
        var stream = System.IO.File.ReadAllBytes("1.csv");
        return new FileStreamResult(new MemoryStream(stream), "text/csv");
    }

    [HttpGet("objectResult1")]
    public IActionResult GetObjectResult1()
    {
        return new ObjectResult(1);
    }

    [HttpGet("objectResult2")]
    public IActionResult GetObjectResult2()
    {
        return new ObjectResult(new { A = 1, B = 2 });
    }

    [HttpGet("pagedResult")]
    public PagedResult<int> GetPagedResult()
    {
        return new PagedResult<int>(1, 10, 10, new[] { 1, 2, 3 });
    }

    [HttpGet("emptyResult")]
    public IActionResult GetEmptyResult()
    {
        return new EmptyResult();
    }
}
