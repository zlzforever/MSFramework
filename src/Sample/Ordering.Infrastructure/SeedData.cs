using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Infrastructure;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var conn = context.Database.GetDbConnection();
        conn.Execute(
            "create table if not exists `user` (`id` varchar(255) not null, `name` varchar(255) not null, primary key(`id`))");
    }
}
