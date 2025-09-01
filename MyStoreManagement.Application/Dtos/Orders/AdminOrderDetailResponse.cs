using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Orders;

// Admin response for viewing detailed order information
public record AdminOrderDetailResponse : AbstractApiResponse<AdminOrderDetailData>
{
    public override AdminOrderDetailData Response { get; set; } = null!;
}

public record AdminOrderDetailData
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    
    // User information
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? UserCreatedAt { get; set; }
    public string UserRole { get; set; } = string.Empty;
    
    // Order details
    public List<AdminOrderItemDetail> OrderItems { get; set; } = new();
    
    // Order statistics
    public int TotalItemsCount { get; set; }
    public decimal AverageItemPrice { get; set; }
}

public record AdminOrderItemDetail
{
    public int OrderDetailId { get; set; }
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice => Quantity * Price;
}
