using MyStoreManagement.Application.Dtos.Orders;

namespace MyStoreManagement.Application.Interfaces.Orders;

public interface IOrderService
{
    Task<OrderCreateResponse> CreateOrderAsync(OrderCreateRequest request);
    
    Task<OrderSelectsResponse> GetOrdersByUserIdAsync(string userId, OrderSelectRequest request);
}