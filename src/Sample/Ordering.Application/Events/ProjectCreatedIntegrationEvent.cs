using System;

namespace Ordering.Application.Events;

public class ProjectCreatedIntegrationEvent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
