using MyStoreManagement.Application.ApiEntities;
using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Orders;

public record OrderSelectsResponse : AbstractApiResponse<PaginationResponse<OrderSelectResponse>>
{
    public override PaginationResponse<OrderSelectResponse> Response { get; set; }
}