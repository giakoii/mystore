using MyStoreManagement.Application.Interfaces.IdentityHepers;
using MyStoreManagement.Application.Interfaces.TokenServices;
using MyStoreManagement.Infrastructure.Identities;
using MyStoreManagement.Infrastructure.Repositories;
using MyStoreManagement.Infrastructure.TokenService;
using Shared.Application.Interfaces.Repositories;

namespace MyStoreManagement.API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Repository services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        
        return services;
    }
}