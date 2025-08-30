namespace MyStoreManagement.Domain.Models;

public partial class SellerDelivery
{
    public int DeliveryId { get; set; }

    public int UserId { get; set; }

    public decimal Quantity { get; set; }

    public virtual User User { get; set; } = null!;
}
