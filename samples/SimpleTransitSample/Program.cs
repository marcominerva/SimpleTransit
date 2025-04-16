using System.Drawing;
using System.Text.RegularExpressions;
using SimpleTransit;
using SimpleTransit.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleTransit(options =>
{
    options.PublishStrategy = PublishStrategy.AwaitWhenAll;

    options.RegisterServiceFromAssemblyContaining<Program>();
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

app.MapPost("api/test", async (INotificationPublisher notificationPublisher) =>
{
    await notificationPublisher.NotifyAsync(new Point(1, 2));
    return TypedResults.Ok();
});

app.Run();

public class Sample : INotificationHandler<Point>, INotificationHandler<Regex>, IDisposable
{
    public void Dispose()
        => throw new NotImplementedException();

    public Task HandleAsync(Point notification, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task HandleAsync(Regex notification, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class Sample2 : INotificationHandler<Point>, IDisposable
{
    public void Dispose()
        => throw new NotImplementedException();

    public Task HandleAsync(Point notification, CancellationToken cancellationToken) => Task.CompletedTask;
}
