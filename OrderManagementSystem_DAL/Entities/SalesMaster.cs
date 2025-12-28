using System;
using System.Collections.Generic;

namespace OrderManagementSystem.Entities;

public partial class SalesMaster
{
    public int Id { get; set; }

    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public int? AgencyId { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual Agency? Agency { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();
}
