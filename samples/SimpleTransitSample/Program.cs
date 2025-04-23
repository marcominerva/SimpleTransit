using SimpleTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleTransit(options =>
{
    options.NotificationPublishStrategy = PublishStrategy.AwaitWhenAll;
    options.ConsumerPublishStrategy = PublishStrategy.AwaitWhenAll;

    options.RegisterServicesFromAssemblyContaining<Program>();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.MapPost("/api/people", async (Person person, INotificationPublisher notificationPublisher) =>
{
    await notificationPublisher.NotifyAsync(person);

    return TypedResults.Created("api/people", person);
});

app.MapPost("/api/products", async (Product product, IMessagePublisher messagePublisher) =>
{
    await messagePublisher.PublishAsync(product);

    return TypedResults.Accepted("api/products", product);
});

app.Run();

public class PersonCreatedNotificationHandler(ILogger<PersonCreatedNotificationHandler> logger) : INotificationHandler<Person>
{
    public Task HandleAsync(Person message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Person created: {FirstName} {LastName} from {City}", message.FirstName, message.LastName, message.City);
        return Task.CompletedTask;
    }
}

public class CreateProductConsumer(ILogger<CreateProductConsumer> logger) : IConsumer<Product>
{
    public async Task HandleAsync(Product message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product: {ProductName}...", message.Name);

        await Task.Delay(2000, cancellationToken);

        logger.LogInformation("Product created: {ProductName} - {ProductDescription} for {Price}", message.Name, message.Description, message.Price.ToString("C"));
    }
}

public record class Person(string FirstName, string LastName, string? City);

public record class Product(string Name, string Description, double Price) : IMessage;