using System;
using System.Collections.Generic;

namespace OrderManagementSystem_DAL.Entities;

public partial class SalesMaster
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public string? BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();
}
