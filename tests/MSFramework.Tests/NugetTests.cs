using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MSFramework.Tests;

public class NugetTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task UnList()
    {
        var numberList = new[]
        {
            "0.15.2",
            //
            "0.15.3-beta1", "0.15.3-beta2", "0.15.3-beta3", "0.15.3-beta4", "0.15.3-beta5", "0.15.3-beta6",
            "0.15.3-beta7", "0.15.3-beta8", "0.15.3-beta9", "0.15.3-beta10",
            //
            "0.15.4-beta01", "0.15.4-beta02", "0.15.4-beta03", "0.15.4-beta04", "0.15.4-beta07", "0.15.4-beta08",
            "0.15.4-beta15", "0.15.4-beta16", "0.15.4-beta17", "0.15.4-beta18", "0.15.4-beta19", "0.15.4-beta22",
            "0.15.4-beta26", "0.15.4-beta29", "0.15.4-beta30"
        };
        var nameList = new[]
        {
            "MSFramework", "MSFramework.Analysis", "MSFramework.Analyzers", "MSFramework.AspNetCore",
            "MSFramework.AspNetCore.Swagger", "MSFramework.Auditing.Loki", "MSFramework.AutoMapper",
            "MSFramework.CAP.Dapr", "MSFramework.Ef", "MSFramework.Ef.Analysis", "MSFramework.Ef.Analyzers",
            "MSFramework.Ef.Design", "MSFramework.Ef.MySql", "MSFramework.Ef.PostgreSql",
            "MSFramework.Ef.SqlServer", "MSFramework.Serialization.Newtonsoft"
        };
        // delete MSFramework.Ef.SqlServer 0.15.2 -Source https://api.nuget.org/v3/index.json -Apikey  -NonInteractive
        var nugetKey = Environment.GetEnvironmentVariable("NUGET_DELETE_KEY");
        foreach (var name in nameList)
        {
            foreach (var number in numberList)
            {
                var arguments =
                    $"delete {name} {number} -Source https://api.nuget.org/v3/index.json -apikey {nugetKey} -NonInteractive";
                var process = Process.Start("nuget", arguments);
                var command = $"nuget {arguments}";
                outputHelper.WriteLine(command);
                Debug.Assert(process != null);
                await process.WaitForExitAsync();
                await Task.Delay(5000);
            }
        }
    }
}
