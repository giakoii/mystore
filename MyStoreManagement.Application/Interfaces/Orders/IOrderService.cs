using MyStoreManagement.Application.Dtos.Orders;

namespace MyStoreManagement.Application.Interfaces.Orders;

public interface IOrderService
{
    Task<OrderCreateResponse> CreateOrderAsync(OrderCreateRequest request);
    
    Task<OrderSelectsResponse> GetOrdersByUserIdAsync(string userId, OrderSelectRequest request);
    
    // Get detailed order information for a specific order (user view)
    Task<OrderDetailFullResponse> GetOrderDetailAsync(int orderId);
    
    // Admin: Get detailed order information with full admin data
    Task<AdminOrderDetailResponse> GetAdminOrderDetailAsync(int orderId);
    
    // Admin: Get all orders from all users with pagination - using dedicated admin response
    Task<AdminOrderSelectResponse> GetAllOrdersAsync(AdminOrderSelectRequest request);
}