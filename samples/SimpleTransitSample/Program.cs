using System.Drawing;
using System.Text.RegularExpressions;
using SimpleTransit;
using SimpleTransit.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleTransit(options =>
{
    options.PublishStrategy = PublishStrategy.AwaitWhenAll;

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

app.MapPost("api/test", async (INotificationPublisher notificationPublisher, IMessagePublisher messagePublisher) =>
{
    //await notificationPublisher.NotifyAsync(new Point(1, 2));

    await messagePublisher.PublishAsync(new TestMessage
    {
        Message = "Hello, world!"
    });

    return TypedResults.Ok();
});

app.Run();

public class Sample : INotificationHandler<Point>, INotificationHandler<Regex>, IConsumer<TestMessage>
{
    public Task HandleAsync(Point notification, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task HandleAsync(Regex notification, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task HandleAsync(TestMessage notification, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class Sample2 : INotificationHandler<Point>, IConsumer<TestMessage>
{
    public Task HandleAsync(Point notification, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task HandleAsync(TestMessage notification, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class TestMessage : IMessage
{
    public string Message { get; set; } = string.Empty;
}