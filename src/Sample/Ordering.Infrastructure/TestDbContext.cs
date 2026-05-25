using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
}
