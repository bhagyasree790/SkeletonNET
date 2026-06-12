var builder = WebApplicationFactory.CreateBuilder();

// Register services with different lifetimes
builder.Services.AddTransient<ITransientService, TransientService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddSingleton<ISingletonService, SingletonService>();
builder.Services.AddScoped<IInternalService, InternalService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<GlobalErrorHandlingMiddleware>();

app.MapGet("/error", ctx =>
{
    throw new Exception("Internal Server Error occurred!");
});

app.MapControllers();

await app.RunAsync(5005);
