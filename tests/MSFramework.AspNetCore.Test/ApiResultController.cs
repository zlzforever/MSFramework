using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MSFramework.AspNetCore.Test;

[Route("[controller]")]
[ApiController]
public class ApiResultController(IOptions<JsonSerializerSettings> options) : ApiControllerBase
{
    public class CustomApiResult<T>(T data) : ApiResult<T>(data)
    {
        public string Marker { get; set; } = "custom";
    }

    public class EnumInput
    {
        /// <summary>
        ///
        /// </summary>
        [Required]
        public State State { get; set; }
    }

    [HttpGet("newtonsoftJson")]
    public string GetNewtonsoftJson()
    {
        return JsonConvert.SerializeObject(new { A = "he", b = 12 }, options.Value);
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

    [HttpGet("objectId")]
    public string GetObjectId(ObjectId id)
    {
        return id.ToString();
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

    [HttpGet("problemDetails")]
    public IActionResult GetProblemDetails()
    {
        var result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "请求无效"
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }

    [HttpGet("ordinaryBadRequest")]
    public IActionResult GetOrdinaryBadRequest()
    {
        return BadRequest("普通请求错误");
    }

    [HttpGet("ordinaryServerError")]
    public IActionResult GetOrdinaryServerError()
    {
        return new ObjectResult("普通服务器错误") { StatusCode = StatusCodes.Status500InternalServerError };
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

    [HttpGet("apiResultGenericSubclass")]
    public CustomApiResult<int> GetApiResultGenericSubclass()
    {
        return new CustomApiResult<int>(7) { Msg = "自定义结果" };
    }

    /// <summary>
    /// 抛出自定义友好异常（业务错误码 2），验证全局异常过滤器返回 HTTP 200 + ApiResult
    /// </summary>
    /// <returns>正常返回不会被执行，始终抛出异常</returns>
    /// <exception cref="MicroserviceFrameworkFriendlyException">模拟接口内抛出友好异常</exception>
    [HttpGet("friendlyException")]
    public int GetFriendlyException()
    {
        throw new MicroserviceFrameworkFriendlyException(2, "业务处理失败");
    }

    /// <summary>
    /// 抛出普通运行时异常（InvalidOperationException），验证全局异常过滤器返回 HTTP 500 + ApiResult
    /// </summary>
    /// <returns>正常返回不会被执行，始终抛出异常</returns>
    /// <exception cref="InvalidOperationException">模拟接口内抛出非友好普通异常</exception>
    [HttpGet("invalidOperationException")]
    public int GetInvalidOperationException()
    {
        throw new InvalidOperationException("系统内部异常");
    }

    /// <summary>
    /// 抛出参数校验异常（ArgumentException），验证全局异常过滤器返回 HTTP 500 + ApiResult
    /// </summary>
    /// <returns>正常返回不会被执行，始终抛出异常</returns>
    /// <exception cref="ArgumentException">模拟接口内抛出参数类普通异常</exception>
    [HttpGet("argumentException")]
    public int GetArgumentException()
    {
        throw new ArgumentException("参数不合法");
    }

    [HttpGet("authenticationException")]
    public int GetAuthenticationException()
    {
        throw new AuthenticationException("认证失败");
    }

    [HttpGet("unauthorizedException")]
    public int GetUnauthorizedException()
    {
        throw new UnauthorizedAccessException("无权访问");
    }

    [HttpGet("notFoundException")]
    public int GetNotFoundException()
    {
        throw new KeyNotFoundException("资源不存在");
    }

    [HttpGet("conflictException")]
    public int GetConflictException()
    {
        throw new MicroserviceFrameworkConflictException("资源状态冲突");
    }

    [HttpGet("unexpectedException")]
    public int GetUnexpectedException()
    {
        throw new Exception("数据库连接字符串和堆栈不应返回给客户端");
    }
}
