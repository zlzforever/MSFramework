using System.Collections.Generic;

namespace MicroserviceFramework.Application;

/// <summary>
/// 
/// </summary>
public interface ISession
{
    string TraceIdentifier { get; }
    string UserId { get; }
    string UserName { get; }
    string Email { get; }
    string PhoneNumber { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Subjects { get; }
}
