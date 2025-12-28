using OrderManagementSystem_Core.Models.Common;
using OrderManagementSystem_Core.Models.OrderListModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_DAL.Repository.Interface
{
    public interface IOrderListRepository
    {
        Task<ResponseModel> GetAllAgencies();
        Task<ResponseModel> GetAllOrderDataByAgency(int agencyId,
            DateTime? fromDate,
            DateTime? toDate);
        Task<ResponseModel> CreateOrder(AddOrderModel model);
        Task<string> GenerateBillNo();
    }
}
