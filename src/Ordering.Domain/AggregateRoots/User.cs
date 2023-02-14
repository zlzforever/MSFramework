using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class User
    : EntityBase<string>
{
    public string Name { get; set; }

    public User(string id) : base(id)
    {
    }
}
