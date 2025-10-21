using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataService _dataService;

        public AdminController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult CoordinatorView()
        {
            var pending = _dataService.GetClaimsByStatus("Pending");
            return View(pending);
        }

        public IActionResult ManagerView()
        {
            var verified = _dataService.GetClaimsByStatus("Verified");
            return View(verified);
        }

        [HttpPost]
        public IActionResult VerifyClaim(int claimId)
        {
            try
            {
                _dataService.UpdateClaimStatus(claimId, "Verified", "Programme Coordinator");
                TempData["SuccessMessage"] = "Claim verified successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error verifying claim: {ex.Message}";
            }
            return RedirectToAction("CoordinatorView");
        }

        [HttpPost]
        public IActionResult ApproveClaim(int claimId)
        {
            try
            {
                _dataService.UpdateClaimStatus(claimId, "Approved", "Academic Manager");
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
            }
            return RedirectToAction("ManagerView");
        }

        [HttpPost]
        public IActionResult RejectClaim(int claimId, string role)
        {
            try
            {
                _dataService.UpdateClaimStatus(claimId, "Rejected", role);
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            }

            return role == "Coordinator"
                ? RedirectToAction("CoordinatorView")
                : RedirectToAction("ManagerView");
        }

        public IActionResult ViewClaim(int id)
        {
            var claim = _dataService.GetClaimById(id);
            if (claim == null)
                return NotFound();

            return View(claim);
        }
    }
}
