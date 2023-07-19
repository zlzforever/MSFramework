using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class User
    :  ExternalEntity<string>
{
    public string Name { get; set; }

    public User(string id) : base(id)
    {
    }
}
