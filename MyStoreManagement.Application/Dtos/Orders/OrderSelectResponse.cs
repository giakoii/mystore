namespace MyStoreManagement.Application.Dtos.Orders;

public class OrderSelectResponse
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public List<OrderDetailResponse> OrderDetails { get; set; } = new();
}

public class OrderDetailResponse
{
    public int OrderDetailId { get; set; }
    public int ProductTypeId { get; set; }
    public string ProductTypeName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice => Quantity * Price;
}