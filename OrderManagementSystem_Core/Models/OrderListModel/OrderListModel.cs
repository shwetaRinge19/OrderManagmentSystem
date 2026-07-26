using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_Core.Models.OrderListModel
{
    public class OrderListModel
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }

        public string? BillNo { get; set; }

        public DateTime? BillDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? CreatedOn { get; set; }

        public List<SalesDetail> SalesDetail { get; set; }
    }

    public class SalesDetail
    {
        public int Id { get; set; }

        public int? AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public int? SalesId { get; set; }

        public string? ItemName { get; set; }

        public int? Quantity { get; set; }

        public decimal? Rate { get; set; }

        public decimal? DiscountRate { get; set; }

        public decimal? Amount { get; set; }

        // Added to match DAL SalesDetail entity and repository usage
        public int? ItemId { get; set; }
        public string? Barcode { get; set; }
    }
}
