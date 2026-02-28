using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_Core.Models.OrderListModel
{
    public class SingleBillPdfVM
    {
        public OrderListModel Order { get; set; }
        public string StoreName { get; set; }
    }
}
