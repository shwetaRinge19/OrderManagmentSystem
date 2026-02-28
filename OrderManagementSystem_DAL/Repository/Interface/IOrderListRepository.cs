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

        Task<ResponseModel> GetAllOrderData(
            DateTime? fromDate,
            DateTime? toDate);
        Task<ResponseModel> GetOrderById(int id);
        Task<ResponseModel> CreateOrder(AddOrderModel model);
        Task<string> GenerateBillNo();
    }
}
