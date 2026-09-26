using Application.Configurations;
using ECommerceApi.ExceptionHandling;
using ECommerceApi.Startup;
using Infrastructure.DependencyInjection;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var envSettings = builder.Configuration.Get<EnvSettings>()
    ?? throw new InvalidOperationException("Environment configuration is required.");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton(envSettings);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(envSettings);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddOpenApi();

var app = builder.Build();

await ECommerceDbInitializer.InitializeAsync(app.Services);

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeLeap E-Commerce API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
