using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Pricings;

public record PricingBatchCreateResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}
