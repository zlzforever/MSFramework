// using System;
// using System.Diagnostics.CodeAnalysis;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.ChangeTracking;
// using Microsoft.EntityFrameworkCore.Infrastructure;
//
// namespace MicroserviceFramework.Ef;
//
// public interface IDbContext : IDisposable,
//     IAsyncDisposable
// {
//     DbSet<TEntity> Set<
//         [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
//                                     DynamicallyAccessedMemberTypes.NonPublicConstructors |
//                                     DynamicallyAccessedMemberTypes.PublicFields |
//                                     DynamicallyAccessedMemberTypes.NonPublicFields |
//                                     DynamicallyAccessedMemberTypes.PublicProperties |
//                                     DynamicallyAccessedMemberTypes.NonPublicProperties |
//                                     DynamicallyAccessedMemberTypes.Interfaces)]
//         TEntity>() where TEntity : class;
//
//     ValueTask<EntityEntry> AddAsync(
//         object entity,
//         CancellationToken cancellationToken = default);
//
//     DatabaseFacade Database { get; }
// }
