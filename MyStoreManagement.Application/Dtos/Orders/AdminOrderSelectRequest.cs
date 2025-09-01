using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Orders;

public class AdminOrderSelectRequest : PaginationRequest
{
    // Optional filter by user ID
    public int? UserId { get; set; }
    
    // Optional filter by date range
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}