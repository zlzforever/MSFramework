using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class EnumInput
    {
        /// <summary>
        ///
        /// </summary>
        [Required]
        public State State { get; set; }
    }

    [HttpPost("enum")]
    public int ValidationEnum([FromBody] EnumInput input)
    {
        return 1;
    }

    [HttpPost("validation")]
    public int Validation([FromQuery, Required] int id)
    {
        return 1;
    }

    [HttpGet("452")]
    public IActionResult Status()
    {
        return new StatusCodeResult(452);
    }

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

    // [HttpGet("ok")]
    // public ApiResult GetOk()
    // {
    //     return ApiResult.Ok;
    // }
    //
    // [HttpGet("error")]
    // public ApiResult GetError()
    // {
    //     return ApiResult.Error;
    // }

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

    // [HttpGet("list2")]
    // public ApiResult<List<int>> GetList2()
    // {
    //     return new ApiResult<List<int>>([1, 2, 3]);
    // }

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
    public PaginationResult<int> GetPagedResult()
    {
        return new PaginationResult<int>(1, 10, 10, [1, 2, 3]);
    }

    [HttpGet("emptyResult")]
    public IActionResult GetEmptyResult()
    {
        return new EmptyResult();
    }

    [HttpGet("apiResult")]
    public ApiResult GetApiResult()
    {
        return new ApiResult() { Data = 1 };
    }

    [HttpGet("apiResultGeneric")]
    public ApiResult<int> GetApiResultGeneric()
    {
        return new ApiResult<int>(1);
    }
}
