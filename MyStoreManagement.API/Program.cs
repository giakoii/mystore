using AuthService.API;
using AuthService.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using MyStoreManagement.API.Extensions;
using MyStoreManagement.API.Helpers;
using MyStoreManagement.Application.Settings;

// Load environment variables
EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

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

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
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

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:7001");
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseStatusCodePages();
app.UseAuthorization();
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi();

app.Run();