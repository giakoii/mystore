using MyStoreManagement.Application.ApiEntities;
using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Orders;

public record OrderSelectsResponse : AbstractApiResponse<PaginationResponse<OrderSelectsDetail>>
{
    public override PaginationResponse<OrderSelectsDetail> Response { get; set; }
}

public record OrderSelectsDetail
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
}