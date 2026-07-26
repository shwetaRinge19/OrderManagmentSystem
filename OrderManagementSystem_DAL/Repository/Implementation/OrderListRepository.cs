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
        //public async Task<ResponseModel> CreateOrder(AddOrderModel model)
        //{
        //    var response = new ResponseModel();

        //    try
        //    {
        //        if (model.SalesDetail == null || !model.SalesDetail.Any())
        //        {
        //            response.IsSuccess = false;
        //            response.Message = "At least one item is required.";
        //            return response;
        //        }

        //        var salesMaster = new SalesMaster
        //        {
        //            BillNo = model.BillNo,
        //            BillDate = model.BillDate ?? DateTime.Now,
        //            CustomerName = model.CustomerName,
        //            CreatedOn = DateTime.Now,
        //            TotalAmount = model.SalesDetail.Sum(x =>
        //                (x.Quantity ?? 0) * (x.Rate ?? 0) - (x.DiscountRate ?? 0))
        //        };

        //        _context.SalesMasters.Add(salesMaster);
        //        await _context.SaveChangesAsync();

        //        foreach (var item in model.SalesDetail)
        //        {
        //            _context.SalesDetails.Add(new Entities.SalesDetail
        //            {
        //                SalesId = salesMaster.Id,
        //                AgencyId = item.AgencyId,
        //                ItemName = item.ItemName,
        //                Quantity = item.Quantity,
        //                Rate = item.Rate,
        //                DiscountRate = item.DiscountRate,
        //                Amount = (item.Quantity ?? 0) * (item.Rate ?? 0)
        //                         - (item.DiscountRate ?? 0)
        //            });
        //        }

        //        await _context.SaveChangesAsync();

        //        response.IsSuccess = true;
        //        response.Message = "Bill created successfully.";
        //    }
        //    catch (Exception ex)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = ex.Message;
        //    }

        //    return response;
        //}

        public async Task<ResponseModel> CreateOrder(AddOrderModel model)
        {
            var response = new ResponseModel();

            if (model.SalesDetail == null || !model.SalesDetail.Any())
            {
                response.IsSuccess = false;
                response.Message = "At least one item is required.";
                return response;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================================================
                // 1. Validate & atomically deduct stock for each scanned/selected item
                //    (skips lines where no ItemId was set, e.g. manual free-text entries)
                // =========================================================
                foreach (var item in model.SalesDetail.Where(x => x.ItemId.HasValue))
                {
                    var qty = item.Quantity ?? 0;

                    if (qty <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = $"Invalid quantity for item '{item.ItemName}'.";
                        await transaction.RollbackAsync();
                        return response;
                    }

                    var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Items
                SET CurrentStock = CurrentStock - {qty}
                WHERE Id = {item.ItemId} AND CurrentStock >= {qty}");

                    if (affectedRows == 0)
                    {
                        var current = await _context.Items
                            .Where(x => x.Id == item.ItemId)
                            .Select(x => new { x.ItemName, x.CurrentStock })
                            .FirstOrDefaultAsync();

                        response.IsSuccess = false;
                        response.Message = current != null
                            ? $"Insufficient stock for '{current.ItemName}'. Available: {current.CurrentStock}, requested: {qty}."
                            : $"Item not found for '{item.ItemName}'.";

                        await transaction.RollbackAsync();
                        return response;
                    }
                }

                // =========================================================
                // 2. Generate bill number safely under lock (inside this same transaction)
                // =========================================================
                var billNo = await GenerateBillNoInternal();

                // =========================================================
                // 3. Create the bill header
                // =========================================================
                var salesMaster = new SalesMaster
                {
                    BillNo = billNo,
                    BillDate = model.BillDate ?? DateTime.Now,
                    CustomerName = model.CustomerName,
                    CreatedOn = DateTime.Now,
                    TotalAmount = model.SalesDetail.Sum(x =>
                        (x.Quantity ?? 0) * (x.Rate ?? 0) * (1 - (x.DiscountRate ?? 0) / 100m))
                };

                _context.SalesMasters.Add(salesMaster);
                await _context.SaveChangesAsync();

                // =========================================================
                // 4. Create line items + stock transaction log entries
                // =========================================================
                foreach (var item in model.SalesDetail)
                {
                    var qty = item.Quantity ?? 0;
                    var rate = item.Rate ?? 0;
                    var discount = item.DiscountRate ?? 0;
                    var amount = qty * rate * (1 - discount / 100m);

                    _context.SalesDetails.Add(new Entities.SalesDetail
                    {
                        SalesId = salesMaster.Id,
                        ItemId = item.ItemId,
                        Barcode = item.Barcode,
                        AgencyId = item.AgencyId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        DiscountRate = item.DiscountRate,
                        Amount = amount
                    });

                    if (item.ItemId.HasValue)
                    {
                        _context.StockTransactions.Add(new StockTransaction
                        {
                            ItemId = item.ItemId.Value,
                            TransactionType = "SALE",
                            Quantity = qty,
                            ReferenceId = salesMaster.Id,
                            Notes = $"Sold via Bill {billNo}",
                            CreatedOn = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.IsSuccess = true;
                response.Message = $"Bill {billNo} created successfully.";
                response.Data = new { BillNo = billNo, salesMaster.Id };
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("UQ_SalesMaster_BillNo") == true)
            {
                await transaction.RollbackAsync();
                response.IsSuccess = false;
                response.Message = "Bill number conflict — please try saving again.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // =========================================================
        // Bill number generation — public preview version (for GET/display only)
        // =========================================================
        public async Task<string> GenerateBillNo()
        {
            var lastBill = await _context.SalesMasters
                .OrderByDescending(x => x.Id)
                .Select(x => x.BillNo)
                .FirstOrDefaultAsync();

            return BuildNextBillNo(lastBill);
        }

        // =========================================================
        // Bill number generation — internal, locked version (used inside CreateOrder's transaction)
        // =========================================================
        private async Task<string> GenerateBillNoInternal()
        {
            var lastBill = await _context.SalesMasters
                .FromSqlRaw(@"
            SELECT TOP 1 * FROM SalesMaster WITH (UPDLOCK, HOLDLOCK)
            ORDER BY Id DESC")
                .Select(x => x.BillNo)
                .FirstOrDefaultAsync();

            return BuildNextBillNo(lastBill);
        }

        // Shared parsing/formatting logic
        private string BuildNextBillNo(string lastBillNo)
        {
            if (string.IsNullOrWhiteSpace(lastBillNo))
                return "BILL-0001";

            var parts = lastBillNo.Split('-');

            // Defensive: if format is ever unexpected, don't crash bill creation
            if (parts.Length < 2 || !int.TryParse(parts[1], out var lastNumber))
                return "BILL-0001";

            return $"BILL-{(lastNumber + 1):D4}";
        }

        // OrderListRepository.cs
        public async Task<ResponseModel> GetItemByBarcode(string barcode)
        {
            var response = new ResponseModel();

            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    response.IsSuccess = false;
                    response.Message = "Barcode is required.";
                    return response;
                }

                var item = await _context.Items
                    .Where(x => x.Barcode == barcode.Trim() && x.IsActive == true)
                    .Select(x => new
                    {
                        x.Id,
                        x.ItemName,
                        x.Rate,
                        x.AgencyId
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No item found for barcode '{barcode}'.";
                    return response;
                }

                response.IsSuccess = true;
                response.Data = item;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
