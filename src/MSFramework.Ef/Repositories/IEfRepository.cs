using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
///
/// </summary>
public interface IEfRepository
{
    /// <summary>
    ///
    /// </summary>
    DbContext DbContext { get; }
}
