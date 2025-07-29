# SimpleTransit

A simple, lightweight implementation of an in-memory publisher/subscriber pattern for .NET applications. SimpleTransit provides a clean and efficient way to implement message-driven architectures using either direct notification handling or queued message processing.

## Features

- **Dual Messaging Patterns**: Support for both immediate notifications and queued message processing
- **Dependency Injection Integration**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **ASP.NET Core Ready**: Built-in support for HTTP context-aware scoping
- **Type-Safe**: Strongly-typed message handling with generic interfaces
- **Minimal Dependencies**: Lightweight with minimal external dependencies
- **Multi-Target Support**: Compatible with .NET 8.0 and .NET 9.0

## Installation

SimpleTransit is available as NuGet packages:

### Main Package
```bash
dotnet add package SimpleTransit
```

### Abstractions Package (for shared contracts)
```bash
dotnet add package SimpleTransit.Abstractions
```

## Quick Start

### 1. Configure Services

Add SimpleTransit to your dependency injection container:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register SimpleTransit and automatically discover handlers/consumers
builder.Services.AddSimpleTransit(options =>
{
    options.RegisterServicesFromAssemblyContaining<Program>();
});

var app = builder.Build();
```

### 2. Define Your Messages

```csharp
// For notifications (direct handling)
public record PersonCreated(string FirstName, string LastName, string? City);

// For queued messages (implement IMessage)
public record ProductCreated(string Name, string Description, double Price) : IMessage;
```

### 3. Create Handlers

#### Notification Handler (Immediate Processing)
```csharp
public class PersonCreatedNotificationHandler(ILogger<PersonCreatedNotificationHandler> logger) : INotificationHandler<PersonCreated>
{
    public async Task HandleAsync(PersonCreated message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Person created: {FirstName} {LastName} from {City}", 
            message.FirstName, message.LastName, message.City);
        
        // Handle the notification immediately
        await DoSomethingAsync(message);
    }
}
```

#### Message Consumer (Queued Processing)
```csharp
public class ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreated>
{
    public async Task HandleAsync(ProductCreated message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing product: {ProductName}...", message.Name);
        
        // Simulate processing
        await Task.Delay(1000, cancellationToken);
        
        logger.LogInformation("Product processed: {ProductName}", message.Name);
    }
}
```

### 4. Publish Messages

```csharp
// In your controllers or services
public class PersonController(
    INotificationPublisher notificationPublisher,
    IMessagePublisher messagePublisher) : ControllerBase
{
    [HttpPost("person")]
    public async Task<IActionResult> CreatePerson(PersonCreated person)
    {
        // Publish notification (handled immediately)
        await notificationPublisher.NotifyAsync(person);
        return Ok();
    }

    [HttpPost("product")]
    public async Task<IActionResult> CreateProduct(ProductCreated product)
    {
        // Publish message (queued for processing)
        await messagePublisher.PublishAsync(product);
        return Accepted();
    }
}
```

## Usage Patterns

### Notifications vs Messages

SimpleTransit supports two distinct messaging patterns:

#### 1. Notifications (Fire-and-Forget)
- **Purpose**: Immediate handling of events
- **Interface**: `INotificationHandler<T>`
- **Publisher**: `INotificationPublisher`
- **Execution**: Synchronous execution of all registered handlers
- **Use Cases**: Logging, immediate side effects, real-time updates

#### 2. Messages (Queued Processing)
- **Purpose**: Reliable, background processing
- **Interface**: `IConsumer<T>` where `T : IMessage`
- **Publisher**: `IMessagePublisher`
- **Execution**: Asynchronous queue processing
- **Use Cases**: Long-running operations, batch processing, reliable delivery

## Configuration Options

### Manual Registration

```csharp
builder.Services.AddSimpleTransit(options =>
{
    // Mark that you have notification handlers
    options.UseNotificationHandlers();
    
    // Mark that you have message consumers
    options.UseMessageConsumers();
});

// Manually register your handlers
builder.Services.AddTransient<INotificationHandler<PersonCreated>, PersonCreatedNotificationHandler>();
builder.Services.AddTransient<IConsumer<ProductCreated>, ProductCreatedConsumer>();
```

### Automatic Registration

```csharp
builder.Services.AddSimpleTransit(options =>
{
    // Register all handlers and consumers from specified assembly
    options.RegisterServicesFromAssemblyContaining<Program>();
    
    // Or from a specific assembly
    options.RegisterServicesFromAssembly(typeof(MyHandler).Assembly);
    
    // With optional filtering
    options.RegisterServicesFromAssembly(
        assembly, 
        type => type.Namespace?.StartsWith("MyApp.Handlers") == true);
});
```

## Advanced Scenarios

### Multiple Handlers
Multiple notification handlers can be registered for the same message type:

```csharp
public class EmailNotificationHandler : INotificationHandler<PersonCreated>
{
    public async Task HandleAsync(PersonCreated message, CancellationToken cancellationToken)
    {
        // Send email
    }
}

public class AuditNotificationHandler : INotificationHandler<PersonCreated>
{
    public async Task HandleAsync(PersonCreated message, CancellationToken cancellationToken)
    {
        // Log to audit system
    }
}
```

### Error Handling
SimpleTransit propagates exceptions from handlers to allow for proper error handling:

```csharp
try
{
    await _notificationPublisher.NotifyAsync(message);
}
catch (Exception ex)
{
    // Handle errors from any of the notification handlers
    _logger.LogError(ex, "Error processing notification");
}
```

### Scoped Services
SimpleTransit properly handles service scoping, especially in ASP.NET Core applications:

```csharp
public class DatabaseHandler(MyDbContext context) : INotificationHandler<PersonCreated>
{
    public async Task HandleAsync(PersonCreated message, CancellationToken cancellationToken)
    {
        // Use scoped DbContext safely
        context.People.Add(new Person { Name = message.FirstName });
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

## Best Practices

1. **Choose the Right Pattern**: Use notifications for immediate actions, messages for background processing
2. **Keep Handlers Focused**: Each handler should have a single responsibility
3. **Handle Errors Gracefully**: Always consider error scenarios in your handlers
4. **Use Cancellation Tokens**: Support cancellation for long-running operations
5. **Leverage Dependency Injection**: Take advantage of scoped services for database operations
6. **Consider Performance**: Notifications are executed synchronously, so keep them fast

## Samples

A complete sample application is available in the [`samples/SimpleTransitSample`](samples/SimpleTransitSample) directory, demonstrating:

- Web API integration
- Both notification and message patterns
- Service registration and configuration
- Real-world usage scenarios

To run the sample:

```bash
cd samples/SimpleTransitSample
dotnet run
```

Then navigate to the Swagger UI at `https://localhost:5001/swagger` to test the API endpoints.

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

### Development Guidelines

1. **Fork the repository** and create a feature branch
2. **Follow existing code style** and conventions
3. **Add tests** for new functionality
4. **Update documentation** as needed
5. **Ensure all tests pass** before submitting

### Building the Project

```bash
# Clone the repository
git clone https://github.com/marcominerva/SimpleTransit.git
cd SimpleTransit

# Build the solution
dotnet build

# Run tests (if available)
dotnet test
```

### Reporting Issues

When reporting issues, please include:
- .NET version
- SimpleTransit version
- Minimal reproduction example
- Expected vs actual behavior

## License

This project is licensed under the [MIT License](LICENSE.txt).
