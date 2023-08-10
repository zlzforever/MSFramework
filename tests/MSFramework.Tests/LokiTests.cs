using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace MSFramework.Tests;

public class LokiTests
{
    [Fact]
    public async Task Send()
    {
        var client = new HttpClient();
        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() + "000000";

        var log =
            $$"""{"streams": [{ "stream": { "foo": "bar2" }, "values": [ [ "{{timestamp}}", "A=B C=E" ] ] }, { "stream": { "foo": "bar2" }, "values": [ [ "{{timestamp}}", "F=G H=J" ] ] }]}""";
        var response = await client.PostAsync("http://192.168.100.254:3100/loki/api/v1/push",
            new StringContent(log, Encoding.UTF8, "application/json"));
        var result = await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public void ChangeTemplate()
    {
        var properties = new List<LogEventProperty> { new("DeviceId", new ScalarValue(ObjectId.GenerateNewId())), };
        var template = new MessageTemplate("{DeviceId}", Array.Empty<MessageTemplateToken>());
        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, template, properties);
        var message = logEvent.MessageTemplate.Render(logEvent.Properties);

    }
}
