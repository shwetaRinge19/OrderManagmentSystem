using System;
using System.Collections.Generic;

namespace OrderManagementSystem_DAL.Entities;

public partial class StockTransaction
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Item Item { get; set; } = null!;
}
