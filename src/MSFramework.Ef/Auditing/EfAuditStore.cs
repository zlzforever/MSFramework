// using System.Threading.Tasks;
// using MicroserviceFramework.Auditing;
//
// namespace MicroserviceFramework.Ef.Auditing;
//
// /// <summary>
// /// EF audit store 不应该和业务共用同一个 Scope 的 db context
// /// </summary>
// public class EfAuditStore<T> : IAuditStore where T : DbContextBase
// {
//     private readonly DbContextBase _dbContext;
//
//     public EfAuditStore(DbContextFactory dbContextFactory)
//     {
//         _dbContext = dbContextFactory.GetDbContext(typeof(T));
//     }
//
//     public async Task AddAsync(AuditOperation auditOperation)
//     {
//         await _dbContext.AddAsync(auditOperation);
//     }
//
//     public async Task SaveChangeAsync()
//     {
//         await _dbContext.CommitAsync();
//     }
//
//     public void Dispose()
//     {
//         _dbContext?.Dispose();
//     }
// }
