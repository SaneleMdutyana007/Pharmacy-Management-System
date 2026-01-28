using Microsoft.AspNetCore.Mvc;
using PharmacyManager.Models;
using System.Diagnostics;

namespace PharmacyManager.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

       
    }
}
