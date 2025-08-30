using AuthService.API;
using AuthService.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MyStoreManagement.API.Extensions;
using MyStoreManagement.API.Helpers;
using MyStoreManagement.Application.Settings;

// Load environment variables
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure URLs for all interfaces
builder.WebHost.UseUrls("http://0.0.0.0:7000", "https://0.0.0.0:7001");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps("certs/aspnetapp.pfx", Environment.GetEnvironmentVariable("CERT_PASSWORD"));
    });
});

// Core services
builder.Services.AddControllers();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<RoleSeederHostedService>();

// Configure services using extension methods
builder.Services.AddDatabaseServices();
builder.Services.AddRepositoryServices();
builder.Services.AddSwaggerServices();
builder.Services.AddCorsServices();
builder.Services.AddAuthenticationServices();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

// Ensure the database is created
await app.EnsureDatabaseCreatedAsync();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseStatusCodePages();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi();
app.Run();
