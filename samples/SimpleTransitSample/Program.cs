using SimpleTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleTransit(options =>
{
    options.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddSingleton<ChatService>();
builder.Services.AddScoped<Service>();
//builder.Services.AddHostedService<CleanupService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
    });
}

app.MapPost("/api/people", async (Person person, ChatService chatService, INotificationPublisher notificationPublisher) =>
{
    var cityInformation = await chatService.AskAsync($"Talk me about {person.City}");

    await notificationPublisher.NotifyAsync(person);
    await notificationPublisher.NotifyAsync(new NewAction("personCreated"));

    return TypedResults.Created("api/people", person);
});

app.MapPost("/api/products", async (Product product, IMessagePublisher messagePublisher) =>
{
    await messagePublisher.PublishAsync(product);

    return TypedResults.Accepted("api/products", product);
});

app.Run();

public class PersonCreatedNotificationHandler(Service service, ILogger<PersonCreatedNotificationHandler> logger) : INotificationHandler<Person>
{
    public async Task HandleAsync(Person message, CancellationToken cancellationToken)
    {
        Console.WriteLine(service.Id);
        logger.LogInformation("Person created: {FirstName} {LastName} from {City}", message.FirstName, message.LastName, message.City);

        await Task.Delay(10000, cancellationToken);
    }
}

public class CreateProductConsumer(Service service, ILogger<CreateProductConsumer> logger) : IConsumer<Product>
{
    public async Task HandleAsync(Product message, CancellationToken cancellationToken)
    {
        Console.WriteLine(service.Id);

        logger.LogInformation("Creating product: {ProductName}...", message.Name);

        await Task.Delay(10000, cancellationToken);

        logger.LogInformation("Product created: {ProductName} - {ProductDescription} for {Price}", message.Name, message.Description, message.Price.ToString("C"));
    }
}

public record class Person(string FirstName, string LastName, string? City);

public record class Product(string Name, string Description, double Price) : IMessage;

public record class NewAction(string ActionName);

public class Service : IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();

    public void Dispose()
    {
    }
}

public class ChatService(INotificationPublisher notificationPublisher)
{
    public async Task<string> AskAsync(string question)
    {
        await Task.Delay(1000);

        await notificationPublisher.NotifyAsync(new NewAction("ask"));

        return string.Empty;
    }
}

public class NewActionNotificationHandler(Service service) : INotificationHandler<NewAction>
{
    public Task HandleAsync(NewAction message, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class CleanupService(INotificationPublisher notificationPublisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken);
            await notificationPublisher.NotifyAsync(new NewAction("cleanup"), stoppingToken);
        }
    }
}