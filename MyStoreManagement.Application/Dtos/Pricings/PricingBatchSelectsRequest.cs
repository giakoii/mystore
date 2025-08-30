using MyStoreManagement.Application.Utils.Paginations;

namespace MyStoreManagement.Application.Dtos.Pricings;

public class PricingBatchSelectsRequest : PaginationRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
