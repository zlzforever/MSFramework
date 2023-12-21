// using DotNetCore.CAP;
// using MicroserviceFramework.Domain;
// using MicroserviceFramework.Ef.Repositories;
// using Microsoft.EntityFrameworkCore.Storage;
//
// namespace Ordering.Application;
//
// public static class RepositoryExtensions
// {
//     public static IDbContextTransaction BeginTransaction(this IRepository repository, ICapPublisher capPublisher,
//         bool autoCommit = false)
//     {
//         if (repository is not IEfRepository efRepository)
//         {
//             return null;
//         }
//
//         var dbContext = efRepository.GetDbContext();
//         return dbContext.Database.BeginTransaction(capPublisher, autoCommit);
//     }
// }
