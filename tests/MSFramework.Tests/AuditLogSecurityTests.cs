using System;
using System.Collections.Generic;
using MicroserviceFramework.Auditing.Loki;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace MSFramework.Tests;

public class AuditLogSecurityTests
{
    [Theory]
    [InlineData("Password")]
    [InlineData("access_token")]
    [InlineData("ConnectionString")]
    [InlineData("ApiKey")]
    public void SanitizeValueMasksSensitiveProperties(string propertyName)
    {
        var sanitizer = new AuditLogSanitizer(new LokiOptions());

        Assert.Equal("[REDACTED]", sanitizer.SanitizeValue(propertyName, "top-secret"));
    }

    [Fact]
    public void SanitizeUrlMasksSensitiveQueryValuesAndCapsLength()
    {
        var sanitizer = new AuditLogSanitizer(new LokiOptions { MaxValueLength = 80 });

        var result = sanitizer.SanitizeUrl("/orders?token=secret&name=visible&password=pwd&connectionString=db");

        Assert.DoesNotContain("secret", result, StringComparison.Ordinal);
        Assert.DoesNotContain("pwd", result, StringComparison.Ordinal);
        Assert.DoesNotContain("connectionString=db", result, StringComparison.Ordinal);
        Assert.Contains("name=visible", result, StringComparison.Ordinal);
        Assert.True(result.Length <= 80);
    }

    [Fact]
    public void SanitizeValueCapsNonSensitiveValues()
    {
        var sanitizer = new AuditLogSanitizer(new LokiOptions { MaxValueLength = 8 });

        Assert.Equal("12345...", sanitizer.SanitizeValue("Description", "1234567890"));
    }

    [Fact]
    public void DisposeFlushesAndDisposesOwnedLoggerPipeline()
    {
        var sink = new RecordingSink();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var store = LokiAuditingStore.Create(logger, new LokiOptions());

        store.Dispose();

        Assert.True(sink.Disposed);
    }

    private sealed class RecordingSink : ILogEventSink, IDisposable
    {
        public bool Disposed { get; private set; }
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);

        public void Dispose() => Disposed = true;
    }
}
