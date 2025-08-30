namespace MyStoreManagement.Application.Dtos.Pricings;

public class PricingBatchCreateRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public List<ProductPriceDetailRequest> PriceDetails { get; set; } = new List<ProductPriceDetailRequest>();
}

public class ProductPriceDetailRequest
{
    public int ProductTypeId { get; set; }
    public decimal Price { get; set; }
}
