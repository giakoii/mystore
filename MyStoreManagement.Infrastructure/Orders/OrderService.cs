using MyStoreManagement.Application.Dtos.Orders;
using MyStoreManagement.Application.Interfaces.Orders;
using MyStoreManagement.Application.Interfaces.Repositories;
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

        try
        {
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

            // Tạo order mới
            var newOrder = new Order
            {
                UserId = userExist.UserId,
                OrderDate = currentDate,
                TotalAmount = 0
            };
            
            await _orderRepository.AddAsync(newOrder);
            await _unitOfWork.SaveChangesAsync();

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            // Tạo order details với giá từ pricing batch
            foreach (var orderDetailRequest in request.OrderDetails)
            {
                var productPrice = await _productPriceRepository
                    .FirstOrDefaultAsync(pp => pp.ProductTypeId == orderDetailRequest.OrderTypeId 
                                             && pp.PricingBatchId == latestPricingBatch.PricingBatchId);

                if (productPrice == null)
                {
                    response.SetMessage(MessageId.E00000, $"Price not found for product type {orderDetailRequest.OrderTypeId}.");
                    return response;
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

            // Thêm order details
            await _orderDetailRepository.AddRangeAsync(orderDetails);
            
            // Cập nhật total amount
            newOrder.TotalAmount = totalAmount;
            
            await _unitOfWork.SaveChangesAsync();

            response.Success = true;
            response.Response = "Order created successfully";
            response.SetMessage(MessageId.I00001, "Order created successfully.");
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error creating order: {ex.Message}");
        }

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

            // Lấy total count
            var totalCount = await _orderRepository.CountAsync(o => o.UserId == userIdInt);

            // Sử dụng PagedAsync để lấy orders với pagination
            var pagedResult = await _orderRepository.PagedAsync(
                request.Page, 
                request.PageSize, 
                o => o.UserId == userIdInt,
                false,
                CancellationToken.None,
                o => o.User,
                o => o.OrderDetails
            );

            // Lấy product types cho order details
            var productTypeIds = pagedResult.Items
                .SelectMany(o => o.OrderDetails)
                .Select(od => od.ProductTypeId)
                .Distinct()
                .ToList();
                
            var productTypes = await _productPriceRepository.ToListAsync(
                pp => productTypeIds.Contains(pp.ProductTypeId),
                false,
                CancellationToken.None,
                pp => pp.ProductType
            );

            var orderResponses = pagedResult.Items.Select(order => new OrderSelectResponse
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                UserName = order.User?.FullName ?? "Unknown",
                PhoneNumber = order.User?.Phone ?? "Unknown",
                OrderDetails = order.OrderDetails.Select(od => {
                    var productPrice = productTypes.FirstOrDefault(pp => pp.ProductTypeId == od.ProductTypeId);
                    return new OrderDetailResponse
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductTypeId = od.ProductTypeId,
                        ProductTypeName = productPrice?.ProductType?.TypeName ?? "Unknown",
                        Quantity = od.Quantity,
                        Price = od.Price
                    };
                }).ToList()
            }).ToList();

            var paginationResponse = new PaginationResponse<OrderSelectResponse>(
                orderResponses, pagedResult.TotalCount, request.Page, request.PageSize);

            response.Success = true;
            response.Response = paginationResponse;
            response.SetMessage(MessageId.I00001, "Orders retrieved successfully.");
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving orders: {ex.Message}");
        }

        return response;
    }
}