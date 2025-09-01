using MyStoreManagement.Application.ApiEntities;
using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Orders;

// Admin response for viewing all orders from all users
public record AdminOrderSelectResponse : AbstractApiResponse<PaginationResponse<AdminOrderDetail>>
{
    public override PaginationResponse<AdminOrderDetail> Response { get; set; } = null!;
}

public record AdminOrderDetail
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    
    // User information for admin view
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Additional order statistics
    public int TotalItems { get; set; }
}
