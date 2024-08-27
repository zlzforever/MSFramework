using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure;

public class OrderingContext(
    DbContextOptions<OrderingContext> options)
    : DbContextBase(options);
