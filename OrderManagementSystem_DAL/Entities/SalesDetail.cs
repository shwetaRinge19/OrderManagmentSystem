using System;
using System.Collections.Generic;

namespace OrderManagementSystem.Entities;

public partial class SalesDetail
{
    public int Id { get; set; }

    public int? SalesId { get; set; }

    public string? ItemName { get; set; }

    public int? Quantity { get; set; }

    public decimal? Rate { get; set; }

    public decimal? DiscountRate { get; set; }

    public decimal? Amount { get; set; }

    public virtual SalesMaster? Sales { get; set; }
}
