using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_Core.Models.OrderListModel
{
    public class IndexViewModel
    {
        public int? SelectedAgencyId { get; set; }

        public List<ListAgencyModel> Agencies { get; set; } = new();
        public List<OrderListModel> Orders { get; set; } = new();
    }
}
