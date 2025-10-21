using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataService _dataService;

        public HomeController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
            return View();
        }
    }
}
