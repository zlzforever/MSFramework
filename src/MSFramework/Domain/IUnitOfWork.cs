using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Audit;

namespace MicroserviceFramework.Domain
{
    public interface IUnitOfWork
    {
        IEnumerable<AuditEntity> GetAuditEntities();

        Task CommitAsync();
    }
}