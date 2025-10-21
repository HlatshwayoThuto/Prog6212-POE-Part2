// Import necessary namespaces
using Microsoft.AspNetCore.Mvc; // Provides classes for building MVC web applications
using WebApplication1.Models;   // Includes application-specific models like DataService

namespace WebApplication1.Controllers
{
    // Defines a controller named AdminController, which inherits from ASP.NET Core's Controller base class
    public class AdminController : Controller
    {
        // Dependency injection of a service class that handles data operations
        private readonly DataService _dataService;

        // Constructor that receives an instance of DataService and assigns it to a private field
        public AdminController(DataService dataService)
        {
            _dataService = dataService;
        }

        // Action method for Programme Coordinators to view claims with "Pending" status
        public IActionResult CoordinatorView()
        {
            // Retrieves all claims with status "Pending" from the data service
            var pending = _dataService.GetClaimsByStatus("Pending");

            // Passes the retrieved claims to the corresponding view for rendering
            return View(pending);
        }

        // Action method for Academic Managers to view claims with "Verified" status
        public IActionResult ManagerView()
        {
            // Retrieves all claims with status "Verified" from the data service
            var verified = _dataService.GetClaimsByStatus("Verified");

            // Passes the verified claims to the corresponding view
            return View(verified);
        }

        // POST action to verify a claim, typically triggered by a form submission
        [HttpPost]
        public IActionResult VerifyClaim(int claimId)
        {
            try
            {
                // Updates the claim status to "Verified" and logs the role as "Programme Coordinator"
                _dataService.UpdateClaimStatus(claimId, "Verified", "Programme Coordinator");

                // Stores a success message in TempData to display after redirection
                TempData["SuccessMessage"] = "Claim verified successfully!";
            }
            catch (Exception ex)
            {
                // Stores an error message in TempData if an exception occurs
                TempData["ErrorMessage"] = $"Error verifying claim: {ex.Message}";
            }

            // Redirects back to the CoordinatorView after processing
            return RedirectToAction("CoordinatorView");
        }

        // POST action to approve a claim, typically used by Academic Managers
        [HttpPost]
        public IActionResult ApproveClaim(int claimId)
        {
            try
            {
                // Updates the claim status to "Approved" and logs the role as "Academic Manager"
                _dataService.UpdateClaimStatus(claimId, "Approved", "Academic Manager");

                // Stores a success message in TempData
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            catch (Exception ex)
            {
                // Stores an error message in TempData if an exception occurs
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
            }

            // Redirects back to the ManagerView after processing
            return RedirectToAction("ManagerView");
        }

        // POST action to reject a claim, used by either Coordinator or Manager
        [HttpPost]
        public IActionResult RejectClaim(int claimId, string role)
        {
            try
            {
                // Updates the claim status to "Rejected" and logs the role (Coordinator or Manager)
                _dataService.UpdateClaimStatus(claimId, "Rejected", role);

                // Stores a success message in TempData
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            catch (Exception ex)
            {
                // Stores an error message in TempData if an exception occurs
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            }

            // Redirects to the appropriate view based on the role
            return role == "Coordinator"
                ? RedirectToAction("CoordinatorView")
                : RedirectToAction("ManagerView");
        }

        // Action method to view details of a specific claim by its ID
        public IActionResult ViewClaim(int id)
        {
            // Retrieves the claim from the data service using the provided ID
            var claim = _dataService.GetClaimById(id);

            // Returns a 404 Not Found response if the claim does not exist
            if (claim == null)
                return NotFound();

            // Passes the claim to the view for detailed display
            return View(claim);
        }
    }
}