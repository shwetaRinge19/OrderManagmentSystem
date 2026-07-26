using System;
using System.Collections.Generic;

namespace OrderManagementSystem_DAL.Entities;

public partial class Item
{
    public int Id { get; set; }

    public string Barcode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public decimal Rate { get; set; }

    public int? AgencyId { get; set; }

    public int CurrentStock { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Agency? Agency { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();

    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
