using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem_DAL.Repository.Interface;

namespace OrderManagementSystem.Controllers
{
    public class AgencyController : Controller
    {
        private readonly IAgencyRepository _agencyRepository;

        public AgencyController(IAgencyRepository agencyRepository)
        {
            _agencyRepository = agencyRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string agencyName)
        {

            if (string.IsNullOrWhiteSpace(agencyName))
            {
                return RedirectToAction("Index", "OrderList");
            }

            var result = await _agencyRepository.AddAgency(agencyName);

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
    }
}
