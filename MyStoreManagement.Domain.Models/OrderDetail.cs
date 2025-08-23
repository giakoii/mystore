using System;
using System.Collections.Generic;

namespace MyStoreManagement.Infrastructure;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductTypeId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductType ProductType { get; set; } = null!;
}
