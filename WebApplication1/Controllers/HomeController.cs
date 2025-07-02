using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EntitiesContext db;
        public HomeController(ILogger<HomeController> logger, EntitiesContext context)
        {
            _logger = logger;
            db = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DashBoard()
        {
            if (User.IsInRole(AppRoles.Admin.ToString()))
            {
                return View();
            }
            else if (User.IsInRole(AppRoles.Doctor.ToString()))
            {
               
                //todo : Statistics endpoint data

                return View();
            }
            return RedirectToAction("Login", "Account");
        }
       
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
