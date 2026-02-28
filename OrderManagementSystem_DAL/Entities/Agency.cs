using System;
using System.Collections.Generic;

namespace OrderManagementSystem_DAL.Entities;

public partial class Agency
{
    public int Id { get; set; }

    public string? AgencyName { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();
}
