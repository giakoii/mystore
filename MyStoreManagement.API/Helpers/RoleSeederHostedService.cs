using Microsoft.EntityFrameworkCore;
using MyStoreManagement.Application.Utils.Const;
using MyStoreManagement.Infrastructure;
using MyStoreManagement.Infrastructure.Contexts;

namespace MyStoreManagement.API.Helpers;

public class RoleSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public RoleSeederHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Starts the hosted service and seeds the required roles into the database.
    /// This method checks if the roles "Admin", "Student", and "Lecturer"
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyStoreManagementContext>();
        var requiredRoles = new[] { ConstRole.Admin, ConstRole.Customer };

        foreach (var roleName in requiredRoles)
        {
            var exists = await context.Roles.Where(r => r.RoleName == roleName).ToListAsync(cancellationToken: cancellationToken);
            if (!exists.Any())
            {
                await context.AddAsync(new Role { RoleName = roleName }, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}