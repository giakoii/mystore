using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Pricings;

public class PricingBatchSelectsRequest : PaginationRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
