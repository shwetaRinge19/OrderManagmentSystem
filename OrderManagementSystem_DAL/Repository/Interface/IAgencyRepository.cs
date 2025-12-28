using OrderManagementSystem_Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_DAL.Repository.Interface
{
    public interface IAgencyRepository
    {
        Task<ResponseModel> AddAgency(string agencyName);
    }
}
