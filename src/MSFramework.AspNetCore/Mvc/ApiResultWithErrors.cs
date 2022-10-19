using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResultWithErrors : ApiResult
{
    public object Errors { get; set; }
}
