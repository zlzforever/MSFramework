using System;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef.Auditing;

public class EfAuditingStore : IAuditingStore
{
    private readonly DbContextFactory _dbContextFactory;
    private readonly AuditingOptions _auditingOptions;

    public EfAuditingStore(DbContextFactory dbContextFactory, IOptionsMonitor<AuditingOptions> auditingOptions)
    {
        _dbContextFactory = dbContextFactory;
        _auditingOptions = auditingOptions.CurrentValue;
    }

    public async Task AddAsync(object sender, AuditOperation auditOperation)
    {
        var auditingDbContext = string.IsNullOrEmpty(_auditingOptions.AuditingDbContextTypeName)
            ? null
            : _dbContextFactory.GetDbContext(Type.GetType(_auditingOptions.AuditingDbContextTypeName));

        var dbContextBase = auditingDbContext ?? sender as DbContext;
        if (dbContextBase != null)
        {
            await dbContextBase.AddAsync(auditOperation);
        }
    }
}
