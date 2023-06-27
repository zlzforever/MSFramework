using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class User
    : EntityBase<string>, IExternalEntity
{
    public string Name { get; set; }

    public User(string id) : base(id)
    {
    }
}
