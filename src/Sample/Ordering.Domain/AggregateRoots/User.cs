using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class UserInfo(string id) : ExternalEntity<string>(id)
{
    public string Name { get; set; }

    public static UserInfo Create(string id, string name)
    {
        return new UserInfo(id) { Name = name };
    }
}
