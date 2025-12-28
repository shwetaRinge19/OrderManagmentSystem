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
        public async Task<IActionResult> Index(int? agencyId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.SelectedAgencyId = agencyId;

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

            if (agencyId.HasValue)
            {
                var orderResponse = await _orderListRepository
                    .GetAllOrderDataByAgency(agencyId.Value, fromDate, toDate);

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

            return RedirectToAction("Index", "OrderList",
                new { agencyId = model.AgencyId });
        }

        public async Task<IActionResult> ExportPdf(
            int? agencyId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!agencyId.HasValue)
                return RedirectToAction(nameof(Index));

            // ===== Agency =====
            var agencyResponse = await _orderListRepository.GetAllAgencies();

            var agencies = agencyResponse.IsSuccess && agencyResponse.Data != null
                ? (List<ListAgencyModel>)agencyResponse.Data
                : new List<ListAgencyModel>();

            var agency = agencies.FirstOrDefault(x => x.Id == agencyId.Value);

            // ===== Orders =====
            var orders = new List<OrderListModel>();

            var orderResponse = await _orderListRepository
                .GetAllOrderDataByAgency(agencyId.Value, fromDate, toDate);

            if (orderResponse.IsSuccess && orderResponse.Data != null)
                orders = (List<OrderListModel>)orderResponse.Data;

            // ===== PDF MODEL =====
            var pdfModel = new OrderReportPdfVM
            {
                AgencyName = agency?.AgencyName ?? "All Agencies",
                FromDate = fromDate?.ToString("dd-MM-yyyy") ?? "-",
                ToDate = toDate?.ToString("dd-MM-yyyy") ?? "-",
                Orders = orders,
                GrandTotal = orders.Sum(x => x.TotalAmount ?? 0) // safe
            };

            return new ViewAsPdf("ExportPdf", pdfModel)
            {
                FileName = $"Order_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,

                // ✅ Page number format: 1 out of 5
                CustomSwitches = "--footer-right \"[page] out of [toPage]\" --footer-font-size 9 --footer-spacing 5"
            };

        }

    }
}
