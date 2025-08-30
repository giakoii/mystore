using Microsoft.EntityFrameworkCore;
using MyStoreManagement.Application.Settings;
using MyStoreManagement.Application.Utils.Const;
using MyStoreManagement.Infrastructure.Contexts;
using Shared.Common.Utils.Const;

namespace MyStoreManagement.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        EnvLoader.Load();
        var connectionString = Environment.GetEnvironmentVariable(ConstEnv.ConnectionDatabase);
        services.AddDbContext<MyStoreManagementContext>(options =>
            options.UseSqlServer(connectionString));
        
        // Entity Framework configuration
        services.AddDbContext<MyStoreManagementContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseOpenIddict();
        });
        return services;
    }
    
    public static async Task<WebApplication> EnsureDatabaseCreatedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyStoreManagementContext>();
        await db.Database.EnsureCreatedAsync();
        return app;
    }
}