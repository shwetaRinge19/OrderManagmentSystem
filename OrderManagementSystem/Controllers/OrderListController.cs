using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem_Core.Models.OrderListModel;
using OrderManagementSystem_DAL.Repository.Interface;
using Rotativa.AspNetCore;

namespace OrderManagementSystem.Controllers
{
    public class OrderListController : Controller
    {
        private readonly IOrderListRepository _orderListRepository;

        public OrderListController(IOrderListRepository orderListRepository)
        {
            _orderListRepository = orderListRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            //ViewBag.SelectedAgencyId = agencyId;

            // ================= AGENCIES =================
            var agencyResponse = await _orderListRepository.GetAllAgencies();

            ViewBag.Agencies = agencyResponse.IsSuccess && agencyResponse.Data != null
                ? (List<ListAgencyModel>)agencyResponse.Data
                : new List<ListAgencyModel>();

            // ================= AUTO BILL NO =================
            ViewBag.BillNo = await _orderListRepository.GenerateBillNo();
            ViewBag.BillDate = DateTime.Now;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");


            // ================= ORDERS =================
            var orders = new List<OrderListModel>();
            decimal grandTotal = 0;

            if (fromDate != null && toDate != null)
            {
                var orderResponse = await _orderListRepository
                    .GetAllOrderData(fromDate, toDate);

                if (orderResponse.IsSuccess && orderResponse.Data != null)
                {
                    orders = (List<OrderListModel>)orderResponse.Data;
                    grandTotal = (decimal)orders.Sum(x => x.TotalAmount);
                }
            }
            else
            {
                var orderResponse = await _orderListRepository
                    .GetAllOrderData(fromDate, toDate);

                if (orderResponse.IsSuccess && orderResponse.Data != null)
                {
                    orders = (List<OrderListModel>)orderResponse.Data;
                    grandTotal = (decimal)orders.Sum(x => x.TotalAmount);
                }
            }

                // ✅ GRAND TOTAL FOR SCREEN
                ViewBag.GrandTotal = grandTotal;

            return View(orders);
        }

        // OrderListController.cs
        [HttpGet]
        public async Task<IActionResult> GetItemByBarcode(string barcode)
        {
            var result = await _orderListRepository.GetItemByBarcode(barcode);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Message });

            return Json(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddOrderModel model)
        {
            var result = await _orderListRepository.CreateOrder(model);

            if (result.IsSuccess)
            {
                TempData["BillStatus"] = "success";
                TempData["BillMessage"] = result.Message;
            }
            else
            {
                TempData["BillStatus"] = "error";
                TempData["BillMessage"] = result.Message;
            }

            return RedirectToAction("Index", "OrderList");
        }

        public async Task<IActionResult> ExportSinglePdf(int id)
        {
            var response = await _orderListRepository.GetOrderById(id);

            if (!response.IsSuccess || response.Data == null)
                return RedirectToAction(nameof(Index));

            var order = (OrderListModel)response.Data;

            var vm = new SingleBillPdfVM
            {
                StoreName = "Keshav Medical Store",
                Order = order
            };

            return new ViewAsPdf("/Views/OrderList/SingleBillPdf.cshtml", vm)
            {
                FileName = $"Bill_{order.BillNo}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches =
                "--footer-right \"Page [page] of [toPage]\" --footer-font-size 9"
            };

        }
        public async Task<IActionResult> ExportPdf(
    DateTime? fromDate,
    DateTime? toDate)
        {
            // ===== Get Orders =====
            var orders = new List<OrderListModel>();

            var orderResponse = await _orderListRepository
                .GetAllOrderData(fromDate, toDate);

            if (orderResponse.IsSuccess && orderResponse.Data != null)
                orders = (List<OrderListModel>)orderResponse.Data;

            // ===== Ensure SalesDetail not null & recalc totals =====
            foreach (var order in orders)
            {
                order.SalesDetail ??= new List<SalesDetail>();

                // Calculate total safely
                order.TotalAmount = order.SalesDetail
                    .Sum(d => d.Amount ?? 0);
            }

            // ===== Grand Total =====
            decimal grandTotal = orders.Sum(o => o.TotalAmount ?? 0);

            // ===== PDF MODEL =====
            var pdfModel = new OrderReportPdfVM
            {
                FromDate = fromDate?.ToString("dd-MM-yyyy") ?? "-",
                ToDate = toDate?.ToString("dd-MM-yyyy") ?? "-",
                Orders = orders,
                GrandTotal = grandTotal
            };

            // ===== Generate PDF =====
            return new ViewAsPdf("ExportPdf", pdfModel)
            {
                FileName = $"Order_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches =
                    "--footer-right \"Page [page] of [toPage]\" " +
                    "--footer-font-size 9 " +
                    "--footer-spacing 5"
            };
        }


    }
}
