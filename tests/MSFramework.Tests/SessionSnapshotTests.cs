using System.Collections.Generic;
using MicroserviceFramework.Application;
using MicroserviceFramework.LocalEvent;
using Xunit;

namespace MSFramework.Tests;

public class SessionSnapshotTests
{
    [Fact]
    public void ConstructorCopiesEverySessionFieldByValue()
    {
        var source = new TestSession();
        var snapshot = new SessionSnapshot(source);

        source.Values[SessionField.DeviceId.HeaderKey] = "mutated";

        Assert.Equal("device-1", snapshot.GetValue(SessionField.DeviceId));
        Assert.Equal("ios", snapshot.GetValue(SessionField.Platform));
        Assert.Equal("35.6812", snapshot.GetValue(SessionField.Latitude));
        Assert.Equal("139.7671", snapshot.GetValue(SessionField.Longitude));
        Assert.Null(snapshot.GetValue(null));
    }

    [Fact]
    public void LoadReplacesEverySessionField()
    {
        var snapshot = new SessionSnapshot(new TestSession());
        var replacement = new TestSession();
        replacement.Values[SessionField.DeviceId.HeaderKey] = "device-2";

        snapshot.Load(replacement);

        Assert.Equal("device-2", snapshot.GetValue(SessionField.DeviceId));
        Assert.Equal("ios", snapshot.GetValue(SessionField.Platform));
    }

    private sealed class TestSession : ISession
    {
        public Dictionary<string, string> Values { get; } = new()
        {
            [SessionField.DeviceId.HeaderKey] = "device-1",
            [SessionField.Platform.HeaderKey] = "ios",
            [SessionField.Latitude.HeaderKey] = "35.6812",
            [SessionField.Longitude.HeaderKey] = "139.7671"
        };

        public string TraceIdentifier => "trace";
        public string UserId => "user";
        public string UserName => "name";
        public string Email => "user@example.invalid";
        public string PhoneNumber => "000";
        public string UserDisplayName => "display";
        public IReadOnlyCollection<string> Roles => ["role"];
        public IReadOnlyCollection<string> Subjects => ["subject"];

        public string GetValue(SessionField field)
        {
            return field != null && Values.TryGetValue(field.HeaderKey, out var value) ? value : null;
        }

        public void Load(ISession session)
        {
        }
    }
}
