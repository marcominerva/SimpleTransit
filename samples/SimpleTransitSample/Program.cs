using System.Drawing;
using SimpleTransit;
using SimpleTransit.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleTransit(options =>
{
    options.UnhandledExceptionBehavior = UnhandledExceptionBehavior.Throw;
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

app.MapPost("api/test", async (INotificator notificator) =>
{
    await notificator.NotifyAsync(new Point(1, 2));
    return TypedResults.Ok();
});

app.Run();

public class Sample : INotificationHandler<Point>, IDisposable
{
    public void Dispose()
        => throw new NotImplementedException();

    public Task HandleAsync(Point notification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
