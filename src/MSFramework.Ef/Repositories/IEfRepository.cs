using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

public interface IEfRepository
{
    DbContext DbContext { get; }
}
