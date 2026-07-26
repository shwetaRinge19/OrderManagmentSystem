using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem_DAL.Entities;
using OrderManagementSystem_Core.Models;

namespace OrderManagementSystem.Controllers
{
    public class ItemController : Controller
    {
        private readonly OrderManagementDbContext _context;

        public ItemController(OrderManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMinimal()
        {
            try
            {
                var items = _context.Items
                    .Where(i => i.IsActive)
                    .Select(i => new
                    {
                        i.Id,
                        i.Barcode,
                        i.ItemName,
                        i.Rate,
                        i.AgencyId
                    })
                    .ToList();

                return Json(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "OrderList");

            try
            {
                var exists = _context.Items.Any(x => x.Barcode == model.Barcode);
                if (exists)
                {
                    TempData["BillStatus"] = "error";
                    TempData["BillMessage"] = "Barcode already exists.";
                    return RedirectToAction("Index", "OrderList");
                }

                var item = new Item
                {
                    Barcode = model.Barcode?.Trim() ?? string.Empty,
                    ItemName = model.ItemName,
                    Rate = model.Rate,
                    AgencyId = model.AgencyId,
                    CurrentStock = model.InitialStock,
                    IsActive = true,
                    CreatedOn = DateTime.Now
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                if (model.InitialStock > 0)
                {
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        ItemId = item.Id,
                        TransactionType = "ADJUST",
                        Quantity = model.InitialStock,
                        ReferenceId = null,
                        Notes = "Initial stock on item creation",
                        CreatedOn = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                }

                TempData["BillStatus"] = "success";
                TempData["BillMessage"] = "Item created successfully.";
            }
            catch (Exception ex)
            {
                TempData["BillStatus"] = "error";
                TempData["BillMessage"] = ex.Message;
            }

            return RedirectToAction("Index", "OrderList");
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(int itemId, int qty)
        {
            if (qty <= 0) return Json(new { isSuccess = false, message = "Quantity must be > 0" });

            var item = await _context.Items.FindAsync(itemId);
            if (item == null) return Json(new { isSuccess = false, message = "Item not found" });

            item.CurrentStock += qty;

            _context.StockTransactions.Add(new StockTransaction
            {
                ItemId = item.Id,
                TransactionType = "ADJUST",
                Quantity = qty,
                ReferenceId = null,
                Notes = "Stock added via UI",
                CreatedOn = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Json(new { isSuccess = true, currentStock = item.CurrentStock });
        }
    }
}
