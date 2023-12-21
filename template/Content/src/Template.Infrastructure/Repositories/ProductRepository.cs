using System.Threading.Tasks;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Infrastructure.Repositories;

public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository
{
    public Task<Product> GetByNameAsync(string name)
    {
        return Store.FirstOrDefaultAsync(x => x.Name == name);
    }
}
