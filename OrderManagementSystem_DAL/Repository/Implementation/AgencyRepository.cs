using Microsoft.EntityFrameworkCore;
using OrderManagementSystem_Core.Models.Common;
using OrderManagementSystem_DAL.Entities;
using OrderManagementSystem_DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_DAL.Repository.Implementation
{
    public class AgencyRepository : IAgencyRepository
    {
        private readonly OrderManagementDbContext _context;

        public AgencyRepository(OrderManagementDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<ResponseModel> AddAgency(string agencyName)
        {
            var response = new ResponseModel();

            try
            {
                if (string.IsNullOrWhiteSpace(agencyName))
                {
                    response.IsSuccess = false;
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    response.Message = "Agency name cannot be empty.";
                    return response;
                }

                // 🔹 Check duplicate
                bool exists = await _context.Agencies
                    .AnyAsync(x => x.AgencyName == agencyName);

                if (exists)
                {
                    response.IsSuccess = false;
                    response.Status = System.Net.HttpStatusCode.Conflict;
                    response.Message = "Agency already exists.";
                    return response;
                }

                _context.Agencies.Add(new Agency
                {
                    AgencyName = agencyName.Trim()
                });

                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Status = System.Net.HttpStatusCode.OK;
                response.Message = "Agency added successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Status = System.Net.HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
            }

            return response;
        }


    }
}
