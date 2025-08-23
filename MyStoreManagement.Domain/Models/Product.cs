using System;
using System.Collections.Generic;

namespace MyStoreManagement.Infrastructure;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();
}
