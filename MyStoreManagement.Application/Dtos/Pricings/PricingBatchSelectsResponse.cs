using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Pricings;

public record PricingBatchSelectsResponse : AbstractApiResponse<List<PricingBatchSelectsEntity>>
{
    public override List<PricingBatchSelectsEntity> Response { get; set; } = new List<PricingBatchSelectsEntity>();
}

public class PricingBatchSelectsEntity
{
    public int PricingBatchId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductPriceEntity> PriceDetails { get; set; } = new List<ProductPriceEntity>();
}

public class ProductPriceEntity
{
    public int PriceId { get; set; }
    public int ProductTypeId { get; set; }
    public string TypeName { get; set; } = null!;
    public decimal Price { get; set; }
}
