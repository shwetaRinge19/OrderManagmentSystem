using Microsoft.EntityFrameworkCore;
using OrderManagementSystem_Core.Models.Common;
using OrderManagementSystem_Core.Models.OrderListModel;
using OrderManagementSystem_DAL.Entities;
using OrderManagementSystem_DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem_DAL.Repository.Implementation
{
    public class OrderListRepository : IOrderListRepository
    {
        private readonly OrderManagementDbContext _context;

        public OrderListRepository(OrderManagementDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseModel> GetAllAgencies()
        {
            var responseModel = new ResponseModel();

            try
            {
                var agencies = await _context.Agencies
                .Select(x => new ListAgencyModel
                {
                    Id = x.Id,
                    AgencyName = x.AgencyName
                })
                .OrderBy(x => x.AgencyName)
                .ToListAsync();

                responseModel.IsSuccess = true;
                responseModel.Message = "Agencies fetched successfully";
                responseModel.Data = agencies;
            }
            catch (Exception ex)
            {
                responseModel.IsSuccess = false;
                responseModel.Message = ex.Message;
                responseModel.Data = null;
            }

            return responseModel;
        }

        public async Task<ResponseModel> GetAllOrderData(DateTime? fromDate,DateTime? toDate)
        {
            var responseModel = new ResponseModel();

            try
            {
                var query = _context.SalesMasters.AsQueryable();

                if (fromDate.HasValue)
                {
                    var from = fromDate.Value.Date;
                    query = query.Where(x => x.BillDate >= from);
                }

                if (toDate.HasValue)
                {
                    var to = toDate.Value.Date.AddDays(1);
                    query = query.Where(x => x.BillDate < to);
                }

                var orders = await query
                    .OrderByDescending(x => x.BillDate)
                    .ThenByDescending(x => x.Id)
                    .Select(x => new OrderListModel
                    {
                        Id = x.Id,
                        CustomerName = x.CustomerName,
                        BillNo = x.BillNo,
                        BillDate = x.BillDate,
                        TotalAmount = x.TotalAmount,
                        CreatedOn = x.CreatedOn,
                        SalesDetail = x.SalesDetails.Select(d => new OrderManagementSystem_Core.Models.OrderListModel.SalesDetail
                        {
                            Id = d.Id,
                            AgencyId = d.AgencyId,
                            AgencyName = _context.Agencies.Where(a => a.Id == d.AgencyId).Select(a => a.AgencyName).FirstOrDefault(),
                            SalesId = d.SalesId,
                            ItemName = d.ItemName,
                            Quantity = d.Quantity,
                            Rate = d.Rate,
                            DiscountRate = d.DiscountRate,
                            Amount = d.Amount
                        }).ToList()
                    })
                    .ToListAsync();

                responseModel.IsSuccess = true;
                responseModel.Data = orders;

            }
            catch (Exception ex)
            {
                responseModel.IsSuccess = false;
                responseModel.Message = ex.Message;
            }

            return responseModel;
        }

        public async Task<ResponseModel> GetOrderById(int id)
        {
            var responseModel = new ResponseModel();

            try
            {
                var order = await _context.SalesMasters
                    .Where(x => x.Id == id)
                    .Select(x => new OrderListModel
                    {
                        Id = x.Id,
                        BillNo = x.BillNo,
                        BillDate = x.BillDate,
                        CustomerName = x.CustomerName,
                        TotalAmount = x.TotalAmount,
                        CreatedOn = x.CreatedOn,
                        SalesDetail = x.SalesDetails.Select(d => new OrderManagementSystem_Core.Models.OrderListModel.SalesDetail
                        {
                            Id = d.Id,
                            AgencyId = d.AgencyId,
                            AgencyName = _context.Agencies
                                .Where(a => a.Id == d.AgencyId)
                                .Select(a => a.AgencyName)
                                .FirstOrDefault(),
                            SalesId = d.SalesId,
                            ItemName = d.ItemName,
                            Quantity = d.Quantity,
                            Rate = d.Rate,
                            DiscountRate = d.DiscountRate,
                            Amount = d.Amount
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                responseModel.IsSuccess = true;
                responseModel.Data = order;
            }
            catch (Exception ex)
            {
                responseModel.IsSuccess = false;
                responseModel.Message = ex.Message;
            }

            return responseModel;
        }

        // 🔹 Create Order (Bill + Items)
        public async Task<ResponseModel> CreateOrder(AddOrderModel model)
        {
            var response = new ResponseModel();

            try
            {
                if (model.SalesDetail == null || !model.SalesDetail.Any())
                {
                    response.IsSuccess = false;
                    response.Message = "At least one item is required.";
                    return response;
                }

                var salesMaster = new SalesMaster
                {
                    BillNo = model.BillNo,
                    BillDate = model.BillDate ?? DateTime.Now,
                    CustomerName = model.CustomerName,
                    CreatedOn = DateTime.Now,
                    TotalAmount = model.SalesDetail.Sum(x =>
                        (x.Quantity ?? 0) * (x.Rate ?? 0) - (x.DiscountRate ?? 0))
                };

                _context.SalesMasters.Add(salesMaster);
                await _context.SaveChangesAsync();

                foreach (var item in model.SalesDetail)
                {
                    _context.SalesDetails.Add(new Entities.SalesDetail
                    {
                        SalesId = salesMaster.Id,
                        AgencyId = item.AgencyId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        DiscountRate = item.DiscountRate,
                        Amount = (item.Quantity ?? 0) * (item.Rate ?? 0)
                                 - (item.DiscountRate ?? 0)
                    });
                }

                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Bill created successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<string> GenerateBillNo()
        {
            var lastBill = await _context.SalesMasters
                .OrderByDescending(x => x.Id)
                .Select(x => x.BillNo)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastBill))
                return "BILL-0001";

            int lastNumber = int.Parse(lastBill.Split('-')[1]);
            return $"BILL-{(lastNumber + 1):D4}";
        }


    }
}
