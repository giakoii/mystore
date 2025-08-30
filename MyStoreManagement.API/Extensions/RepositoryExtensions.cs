using MyStoreManagement.Application.Interfaces.IdentityHepers;
using MyStoreManagement.Application.Interfaces.Orders;
using MyStoreManagement.Application.Interfaces.Pricings;
using MyStoreManagement.Application.Interfaces.ProductTypes;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Application.Interfaces.TokenServices;
using MyStoreManagement.Application.Interfaces.Users;
using MyStoreManagement.Domain.Models;
using MyStoreManagement.Infrastructure;
using MyStoreManagement.Infrastructure.Identities;
using MyStoreManagement.Infrastructure.Orders;
using MyStoreManagement.Infrastructure.Pricings;
using MyStoreManagement.Infrastructure.ProductTypes;
using MyStoreManagement.Infrastructure.Repositories;
using MyStoreManagement.Infrastructure.TokenService;
using MyStoreManagement.Infrastructure.Users;

namespace MyStoreManagement.API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Repository services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IRepository<User>, BaseRepository<User>>();
        services.AddScoped<IRepository<Role>, BaseRepository<Role>>();
        services.AddScoped<IRepository<ProductType>, BaseRepository<ProductType>>();
        services.AddScoped<IRepository<PricingBatch>, BaseRepository<PricingBatch>>();
        services.AddScoped<IRepository<ProductPrice>, BaseRepository<ProductPrice>>();
        services.AddScoped<IRepository<Order>, BaseRepository<Order>>();
        services.AddScoped<IRepository<OrderDetail>, BaseRepository<OrderDetail>>();
        
        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductTypeService, ProductTypeService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IOrderService, OrderService>();
        
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        
        return services;
    }
}