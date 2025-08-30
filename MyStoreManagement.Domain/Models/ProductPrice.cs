namespace MyStoreManagement.Domain.Models;

public partial class ProductPrice
{
    public int PriceId { get; set; }

    public int ProductTypeId { get; set; }
    
    public int PricingBatchId { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public virtual ProductType ProductType { get; set; } = null!;

    public virtual PricingBatch PricingBatch { get; set; } = null!;
}
