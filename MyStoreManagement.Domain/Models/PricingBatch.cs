namespace MyStoreManagement.Domain.Models;

public partial class PricingBatch
{
    public int PricingBatchId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}