using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Orders;

// Response for detailed order information
public record OrderDetailFullResponse : AbstractApiResponse<OrderDetailData>
{
    public override OrderDetailData Response { get; set; } = null!;
}

public record OrderDetailData
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<OrderItemDetail> OrderItems { get; set; } = new();
}

public record OrderItemDetail
{
    public int OrderDetailId { get; set; }
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice => Quantity * Price;
}
