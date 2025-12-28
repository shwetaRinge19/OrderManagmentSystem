using System;
using System.Collections.Generic;

namespace OrderManagementSystem.Entities;

public partial class Agency
{
    public int Id { get; set; }

    public string? AgencyName { get; set; }

    public virtual ICollection<SalesMaster> SalesMasters { get; set; } = new List<SalesMaster>();
}
