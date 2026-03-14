# MSFramework - Agent Guidelines

A lightweight DDD microservice framework for .NET 8/10. This document guides AI coding agents working in this repository.

## Build, Lint, and Test Commands

```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj

# Run a single test class
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj --filter "FullyQualifiedName~DomainEventTests"

# Run a single test method
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj --filter "FullyQualifiedName~DomainEventTests.DispatchTo1HandlerTests"

# Run tests with verbose output
dotnet test tests/MSFramework.Tests/MSFramework.Tests.csproj -v n

# EF Core migrations (from project root)
dotnet ef migrations add Init -s src/Ordering.API -c OrderingContext -p src/Ordering.Infrastructure

# Generate SQL migration script
dotnet ef migrations script -s src/Ordering.API -c OrderingContext -p src/Ordering.Infrastructure

# Optimize EF Core compiled models
dotnet ef dbcontext optimize -s src/Ordering.API -c OrderingContext -p src/Ordering.Infrastructure -o CompileModels -n Ordering.Infrastructure.CompileModels

# Pack NuGet packages
dotnet pack --configuration Release
```

## Project Structure

The solution follows DDD (Domain-Driven Design) layered architecture:

```
src/
├── MSFramework/                    # Core framework (domain primitives, utilities)
├── MSFramework.AspNetCore/         # ASP.NET Core integration
├── MSFramework.Ef/                 # EF Core repository implementations
├── MSFramework.Ef.SqlServer/       # SQL Server provider
├── MSFramework.Ef.MySql/           # MySQL provider
├── MSFramework.Ef.PostgreSql/      # PostgreSQL provider
├── MSFramework.AutoMapper/         # AutoMapper integration
├── MSFramework.Serialization.Newtonsoft/  # JSON serialization
├── Ordering.Domain/                # Domain layer (entities, value objects, domain events)
├── Ordering.Application/           # Application layer (commands, queries, handlers)
├── Ordering.Infrastructure/        # Infrastructure layer (repositories, DbContext)
└── Ordering.API/                   # API layer (controllers)

tests/
├── MSFramework.Tests/              # Unit tests
└── MSFramework.AspNetCore.Test/    # Integration tests
```

## Code Style Guidelines

### Formatting (from .editorconfig)

- **Indentation**: Spaces, 4 spaces for C# code, 2 spaces for XML/JSON
- **Line endings**: UTF-8, insert final newline, trim trailing whitespace
- **Braces**: Allman style (new line before open brace for all constructs)
- **var**: Prefer `var` for all variable declarations
- **System imports**: Sort System.* directives first
- **PascalCase**: For constant fields and all public members

### Naming Conventions

```csharp
// Interfaces: Prefix with 'I'
public interface IRepository { }
public interface IOrderRepository { }

// Classes: PascalCase
public class OrderController { }
public class CreateOrderCommandHandler { }

// Records for DTOs/Commands/Events
public record CreateOrderCommand(string UserId, string City) : Request<string>;
public record OrderStartedDomainEvent : DomainEvent;

// Private fields: Underscore prefix in constructors, or use primary constructors
private readonly IOrderRepository _orderRepository;  // Traditional
public class Handler(IOrderRepository orderRepository)  // Primary constructor (preferred)

// Properties: PascalCase
public string Name { get; private set; }
public IReadOnlyCollection<OrderItem> Items => _items;

// Methods: PascalCase, async suffix for async methods
public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
```

### Import Organization

```csharp
// Order: System.* first, then third-party, then project namespaces
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregateRoots;
```

## Architecture Patterns

### Aggregate Roots

Inherit from framework base classes based on auditing needs:

```csharp
// With creation audit only
public class Entity1(int id) : CreationAggregateRoot<int>(id);

// With creation and modification audit
public class Product : ModificationAggregateRoot
{
    private Product(ObjectId id) : base(id) { }
    
    public static Product New(string name, int price)
    {
        var product = new Product(ObjectId.GenerateNewId())
        {
            Name = name,
            Price = price
        };
        product.AddDomainEvent(new ProjectCreatedEvent { Id = product.Id });
        return product;
    }
    
    public string Name { get; private set; }
}

// With full audit (creation, modification, deletion)
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    // Private constructor for EF Core
    private Order(string id) : base(id) { }
    
    // Factory method for domain creation
    public static Order Create(UserInfo buyer, Address address, string description)
    {
        var order = new Order(ObjectId.GenerateNewId().ToString()) { ... };
        order.AddDomainEvent(new OrderStartedDomainEvent(order, buyer.Id));
        return order;
    }
}
```

### Value Objects

Use records for value objects:

```csharp
public abstract record ValueObject
{
    public ValueObject Copy() => this with { };
}

public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
}
```

### Domain Events

```csharp
// Define domain events as records
public record OrderStartedDomainEvent : DomainEvent;

// Raise events from aggregate roots
public void SetCancelledStatus()
{
    Status = OrderStatus.Cancelled;
    AddDomainEvent(new OrderCancelledDomainEvent(this));
}
```

### Commands and Handlers (CQRS)

```csharp
// Command definition
public record CreateOrderCommand(
    List<OrderItemDTO> OrderItems,
    string UserId) : Request<string>;

// Command handler using primary constructor
public class CreateOrderCommandHandler(IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, string>
{
    public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(...);
        await orderRepository.AddAsync(order);
        return order.Id.ToString();
    }
}
```

### Repositories

```csharp
// Domain layer: Define interface
public interface IProductRepository
{
    Task<Product> GetByNameAsync(string name);
}

// Infrastructure layer: Implement using EF
public class ProductRepository(DbContextFactory dbContextFactory)
    : EfRepository<Product, ObjectId>(dbContextFactory), IProductRepository
{
    public Task<Product> GetByNameAsync(string name)
    {
        return Store.FirstOrDefaultAsync(x => x.Name == name);
    }
}
```

### API Controllers

```csharp
[Route("api/v1.0/orders")]
[ApiController]
public class OrderController(
    IOrderRepository orderRepository,
    IMediator mediator,
    ILogger<OrderController> logger) : ApiControllerBase
{
    [HttpPost]
    public async Task<string> Create([FromBody] CreateOrderCommand command)
    {
        return await mediator.SendAsync(command);
    }

    [HttpGet("{orderId}")]
    public async Task<OrderDto> GetAsync([FromRoute, Required, StringLength(36)] string orderId)
    {
        var order = await orderingQuery.GetAsync(orderId);
        return objectAssembler.To<OrderDto>(order);
    }
}
```

## Error Handling

### Custom Exceptions

```csharp
// Framework exception with error code
public class MicroserviceFrameworkException(int code, string message, Exception innerException = null)
    : ApplicationException(message, innerException)
{
    public int Code { get; } = code;
}

// User-friendly exception (for API responses)
public class MicroserviceFrameworkFriendlyException : MicroserviceFrameworkException;

// Domain-specific exceptions
public class OrderingDomainException : Exception
{
    public OrderingDomainException(string msg) : base(msg) { }
}
```

### Argument Validation

```csharp
using MicroserviceFramework.Utils;

Check.NotNull(value, nameof(value));
Check.NotNullOrEmpty(value, nameof(value));
```

## Dependency Injection

### Auto-Registration via Marker Interfaces

```csharp
// Transient lifetime
public interface ITransientDependency;

// Scoped lifetime
public interface IScopeDependency;

// Singleton lifetime
public interface ISingletonDependency;

// Usage: Implement marker interface for auto-registration
public class MyService : ITransientDependency { }
```

### Manual Registration

```csharp
services.AddMicroserviceFramework(x =>
{
    x.UseDependencyInjectionLoader();
    x.UseAspNetCoreExtension();
});
```

## Testing

- Framework: xUnit
- Patterns: Arrange-Act-Assert

```csharp
public class DomainEventTests
{
    [Fact]
    public async Task DispatchTo1HandlerTests()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMicroserviceFramework(x =>
        {
            x.UseDependencyInjectionLoader();
            x.UseAspNetCoreExtension();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.UseMicroserviceFramework();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        await mediator.PublishAsync(new DomainEvent4());

        // Assert
        Assert.Equal(1, DomainEvent4.Count);
    }
}
```

## Key Dependencies

- **.NET**: 8.0 (stable), 10.0 (in testing)
- **EF Core**: 9.x
- **MongoDB.Bson**: For ObjectId support
- **AutoMapper**: Object mapping
- **xUnit**: Testing framework
- **Dapr**: Distributed application runtime

## Common Patterns

- Use **primary constructors** (C# 12) for dependency injection
- Use **records** for DTOs, commands, queries, and events
- Use **factory methods** (`Create`, `New`) for domain object construction
- Keep **domain logic** inside aggregate roots (rich domain model)
- Use **IObjectAssembler** for DTO mapping, not manual mapping
- Always use **CancellationToken** in async methods
- Use **[Required]** and **[StringLength]** attributes for API validation
