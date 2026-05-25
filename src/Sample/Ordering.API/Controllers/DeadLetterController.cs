using System.IO;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ordering.API.Controllers;

[Route("api/v1.0/deadLetters")]
[ApiController]
public class DeadLetterController() : ApiControllerBase
{
    [Topic("rabbitmq-pubsub", "biz.ordering.dead-letter")]
    [HttpPost]
    public async Task OnOrderCreatedAsync()
    {
        var streamReader = new StreamReader(HttpContext.Request.Body);
        var body = await streamReader.ReadToEndAsync();
        Logger.LogError("接收到死信： {DeadLetter}", body);
    }
}
