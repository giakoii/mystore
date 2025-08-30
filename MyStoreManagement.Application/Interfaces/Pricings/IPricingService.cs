using MyStoreManagement.Application.Dtos.Pricings;

namespace MyStoreManagement.Application.Interfaces.Pricings;

public interface IPricingService
{
    Task<PricingBatchCreateResponse> CreatePricingBatchAsync(PricingBatchCreateRequest request);
    Task<PricingBatchSelectsResponse> GetPricingBatchesAsync(PricingBatchSelectsRequest request);
}
