using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly EntitiesContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(EntitiesContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();

            IQueryable<Patient> patientsQuery = _context.Patients;
            IQueryable<Test> testsQuery = _context.Tests;

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId = _userManager.GetUserId(User);
                patientsQuery = patientsQuery.Where(p => p.DocID == currentDoctorId);
                testsQuery = testsQuery.Where(t => t.Patient.DocID == currentDoctorId);
            }

            viewModel.TotalPatients = await patientsQuery.CountAsync();
            viewModel.TotalTests = await testsQuery.CountAsync();
            viewModel.PositiveCases = await testsQuery.CountAsync(t => t.Result == "Parasitized");
            viewModel.NegativeCases = await testsQuery.CountAsync(t => t.Result == "Uninfected");

            viewModel.PieChartLabels.AddRange(new string[] { "Malaria (Positive)", "Healthy (Negative)" });
            viewModel.PieChartData.AddRange(new int[] { viewModel.PositiveCases, viewModel.NegativeCases });

            var testsPerDay = await testsQuery
                .Where(t => t.Date >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(t => t.Date.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .OrderBy(x => x.Day)
                .ToListAsync();

            foreach (var item in testsPerDay)
            {
                viewModel.LineChartLabels.Add(item.Day.ToString("MMM d"));
                viewModel.LineChartData.Add(item.Count);
            }

            return View(viewModel);
        }
    }
}