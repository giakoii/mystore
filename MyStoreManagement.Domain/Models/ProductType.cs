using System;
using System.Collections.Generic;

namespace MyStoreManagement.Infrastructure;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public int ProductId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}
