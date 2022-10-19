using System.Collections.Generic;

namespace MicroserviceFramework.AspNetCore.Mvc;

public class ApiResultWithErrors : ApiResult
{
    public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}
