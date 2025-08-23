using Microsoft.EntityFrameworkCore;
using MyStoreManagement.Infrastructure.Contexts;
using Shared.Common.Utils.Const;

namespace MyStoreManagement.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConstEnv.AuthServiceDB);
        
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