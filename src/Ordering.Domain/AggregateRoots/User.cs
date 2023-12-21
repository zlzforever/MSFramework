using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class User(int id) : ExternalEntity<int>(id)
{
    public string Name { get; set; }
}
