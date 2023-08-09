using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
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
}
