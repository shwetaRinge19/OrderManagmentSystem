using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_Core.Models.OrderListModel
{
    public class OrderReportPdfVM
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public decimal GrandTotal { get; set; }   // ✅ NEW
        public List<OrderListModel> Orders { get; set; }
    }
}
