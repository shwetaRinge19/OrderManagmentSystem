using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_Core.Models.OrderListModel
{
    public class AddOrderModel
    {

        public string? BillNo { get; set; }

        public DateTime? BillDate { get; set; }

        public int? AgencyId { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? CreatedOn { get; set; }

        public List<SalesDetail> SalesDetail { get; set; } = new List<SalesDetail>();
 
    }

}
