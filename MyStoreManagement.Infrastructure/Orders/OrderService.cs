using MyStoreManagement.Application.Dtos.Orders;
using MyStoreManagement.Application.Interfaces.Orders;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Application.Utils;
using MyStoreManagement.Application.Utils.Paginations;
using MyStoreManagement.Domain.Models;
using Shared.Application.Utils.Const;

namespace MyStoreManagement.Infrastructure.Orders;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    private readonly IRepository<PricingBatch> _pricingBatchRepository;
    private readonly IRepository<ProductPrice> _productPriceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IRepository<Order> orderRepository, 
        IRepository<User> userRepository, 
        IRepository<OrderDetail> orderDetailRepository,
        IRepository<PricingBatch> pricingBatchRepository,
        IRepository<ProductPrice> productPriceRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderDetailRepository = orderDetailRepository;
        _pricingBatchRepository = pricingBatchRepository;
        _productPriceRepository = productPriceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderCreateResponse> CreateOrderAsync(OrderCreateRequest request)
    {
        var response = new OrderCreateResponse { Success = false };

        var userExist = await _userRepository.FirstOrDefaultAsync(x => x.Phone == request.PhoneNumber);
        if (userExist == null)
        {
            response.SetMessage(MessageId.E00000, "User with this phone number does not exist.");
            return response;
        }

        var currentDate = DateTime.UtcNow;

        // Lấy pricing batch mới nhất
        var latestPricingBatch = (await _pricingBatchRepository
                .ToListAsync(pb => pb.CreatedAt <= currentDate))
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefault();

        if (latestPricingBatch == null)
        {
            response.SetMessage(MessageId.E00000, "No pricing batch available.");
            return response;
        }

        await _unitOfWork.BeginTransactionAsync(async () =>
        {
            // Tạo order mới
            var newOrder = new Order
            {
                UserId = userExist.UserId,
                OrderDate = StringUtil.GetVietnamTime(),
                TotalAmount = 0
            };

            await _orderRepository.AddAsync(newOrder);
            await _unitOfWork.SaveChangesAsync();

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            // Createn order details
            foreach (var orderDetailRequest in request.OrderDetails)
            {
                var productPrice = await _productPriceRepository
                    .FirstOrDefaultAsync(pp => pp.ProductTypeId == orderDetailRequest.OrderTypeId
                                               && pp.PricingBatchId == latestPricingBatch.PricingBatchId);

                if (productPrice == null)
                {
                    response.SetMessage(MessageId.E00000,
                        $"Price not found for product type {orderDetailRequest.OrderTypeId}.");
                    return false;
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    ProductTypeId = orderDetailRequest.OrderTypeId,
                    Quantity = orderDetailRequest.Quantity,
                    Price = productPrice.Price
                };

                orderDetails.Add(orderDetail);
                totalAmount += orderDetail.Quantity * orderDetail.Price;
            }

            // Add order details to repository
            await _orderDetailRepository.AddRangeAsync(orderDetails);

            // Update total amount for the order
            newOrder.TotalAmount = totalAmount;

            await _unitOfWork.SaveChangesAsync();

            // True
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }

    public async Task<OrderSelectsResponse> GetOrdersByUserIdAsync(string userId, OrderSelectRequest request)
    {
        var response = new OrderSelectsResponse { Success = false };

        try
        {
            if (!int.TryParse(userId, out var userIdInt))
            {
                response.SetMessage(MessageId.E00000, "Invalid user ID.");
                return response;
            }
            
            // Use PagedAsync to get paginated orders with related user and order details
            var pagedResult = await _orderRepository.PagedAsync(
                request.Page, 
                request.PageSize, 
                o => o.UserId == userIdInt,
                false,
                CancellationToken.None,
                orderBy: x => x.OrderByDescending(o => o.OrderDate),
                o => o.User
            );

            var orderResponses = pagedResult.Items.Select(order => new OrderSelectsDetail
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
            }).ToList();

            var paginationResponse = new PaginationResponse<OrderSelectsDetail>(
                orderResponses, pagedResult.TotalCount, request.Page, request.PageSize);

            // True
            response.Success = true;
            response.Response = paginationResponse;
            response.SetMessage(MessageId.I00001);
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving orders: {ex.Message}");
        }

        return response;
    }

    // Get detailed order information for a specific order
    public async Task<OrderDetailFullResponse> GetOrderDetailAsync(int orderId)
    {
        var response = new OrderDetailFullResponse { Success = false };

        try
        {
            // Get order with related user and order details
            var order = await _orderRepository.FirstOrDefaultAsync(
                o => o.OrderId == orderId,
                false,
                CancellationToken.None,
                o => o.User,
                o => o.OrderDetails
            );

            if (order == null)
            {
                response.SetMessage(MessageId.E00000, "Order not found.");
                return response;
            }

            // Get product type information for order details
            var productTypeIds = order.OrderDetails.Select(od => od.ProductTypeId).Distinct().ToList();
            var productTypes = await _productPriceRepository.ToListAsync(
                pp => productTypeIds.Contains(pp.ProductTypeId),
                false,
                CancellationToken.None,
                pp => pp.ProductType
            );

            var productTypeDict = productTypes
                .GroupBy(pp => pp.ProductTypeId)
                .ToDictionary(g => g.Key, g => g.First().ProductType?.TypeName);

            var orderDetailData = new OrderDetailData
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                UserName = order.User?.FullName!,
                PhoneNumber = order.User?.Phone!,
                OrderItems = order.OrderDetails.Select(od => new OrderItemDetail
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductTypeId = od.ProductTypeId,
                    ProductTypeName = productTypeDict.GetValueOrDefault(od.ProductTypeId, "Unknown"),
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList()
            };

            response.Success = true;
            response.Response = orderDetailData;
            response.SetMessage(MessageId.I00001);
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving order details: {ex.Message}");
        }

        return response;
    }

    // Admin: Get all orders from all users with pagination
    public async Task<AdminOrderSelectResponse> GetAllOrdersAsync(AdminOrderSelectRequest request)
    {
        var response = new AdminOrderSelectResponse { Success = false };

        try
        {
            // Build filter expression
            var filter = BuildOrderFilter(request);

            // Use PagedAsync to get paginated orders with related user and order details information
            var pagedResult = await _orderRepository.PagedAsync(
                request.Page,
                request.PageSize,
                filter,
                false,
                CancellationToken.None,
                orderBy: x => x.OrderByDescending(o => o.OrderDate),
                o => o.User,
                o => o.OrderDetails
            );

            var adminOrderDetails = pagedResult.Items.Select(order => new AdminOrderDetail
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                UserId = order.UserId,
                UserName = order.User?.FullName ?? "Unknown",
                PhoneNumber = order.User?.Phone ?? "Unknown",
                TotalItems = order.OrderDetails?.Count ?? 0
            }).ToList();

            var paginationResponse = new PaginationResponse<AdminOrderDetail>(
                adminOrderDetails, pagedResult.TotalCount, request.Page, request.PageSize);

            response.Success = true;
            response.Response = paginationResponse;
            response.SetMessage(MessageId.I00001);
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving all orders: {ex.Message}");
        }

        return response;
    }

    // Admin: Get detailed order information with full admin data
    public async Task<AdminOrderDetailResponse> GetAdminOrderDetailAsync(int orderId)
    {
        var response = new AdminOrderDetailResponse { Success = false };

        try
        {
            // Get order with all related data for admin view
            var order = await _orderRepository.FirstOrDefaultAsync(
                o => o.OrderId == orderId,
                false,
                CancellationToken.None,
                o => o.User,
                o => o.User.Role,
                o => o.OrderDetails
            );

            if (order == null)
            {
                response.SetMessage(MessageId.E00000, "Order not found.");
                return response;
            }

            // Get product type and product information for order details
            var productTypeIds = order.OrderDetails.Select(od => od.ProductTypeId).Distinct().ToList();
            var productTypes = await _productPriceRepository.ToListAsync(
                pp => productTypeIds.Contains(pp.ProductTypeId),
                false,
                CancellationToken.None,
                pp => pp.ProductType
            );

            var productTypeDict = productTypes
                .GroupBy(pp => pp.ProductTypeId)
                .ToDictionary(g => g.Key, g => new { 
                    TypeName = g.First().ProductType?.TypeName ?? "Unknown",
                });

            // Calculate order statistics
            var totalItemsCount = order.OrderDetails.Count;
            var averageItemPrice = totalItemsCount > 0 ? 
                order.OrderDetails.Average(od => od.Price) : 0;

            var adminOrderDetailData = new AdminOrderDetailData
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                UserId = order.UserId,
                UserName = order.User?.FullName ?? "Unknown",
                PhoneNumber = order.User?.Phone ?? "Unknown",
                UserCreatedAt = order.User?.CreatedAt,
                UserRole = order.User?.Role?.RoleName ?? "Unknown",
                TotalItemsCount = totalItemsCount,
                AverageItemPrice = averageItemPrice,
                OrderItems = order.OrderDetails.Select(od => new AdminOrderItemDetail
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductTypeId = od.ProductTypeId,
                    ProductTypeName = productTypeDict.GetValueOrDefault(od.ProductTypeId)?.TypeName ?? "Unknown",
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList()
            };

            response.Success = true;
            response.Response = adminOrderDetailData;
            response.SetMessage(MessageId.I00001);
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving admin order details: {ex.Message}");
        }

        return response;
    }

    // Helper method to build filter expression for admin order queries
    private System.Linq.Expressions.Expression<Func<Order, bool>> BuildOrderFilter(AdminOrderSelectRequest request)
    {
        DateTime? fromDateTime = request.FromDate?.ToDateTime(TimeOnly.MinValue);
        DateTime? toDateTime = request.ToDate?.ToDateTime(TimeOnly.MaxValue);

        return order =>
            (request.UserId == null || order.UserId == request.UserId) &&
            (fromDateTime == null || order.OrderDate >= fromDateTime) &&
            (toDateTime == null || order.OrderDate <= toDateTime);
    }
}