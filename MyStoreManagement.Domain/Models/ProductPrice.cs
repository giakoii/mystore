using System;
using System.Collections.Generic;

namespace MyStoreManagement.Infrastructure;

public partial class ProductPrice
{
    public int PriceId { get; set; }

    public int ProductTypeId { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ProductType ProductType { get; set; } = null!;
}
