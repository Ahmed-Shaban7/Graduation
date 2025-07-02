using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DoctorPatientDashboard.Controllers
{
    public class DoctorController : Controller
    {
        private readonly EntitiesContext db ;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DoctorController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EntitiesContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = context;
        }


        [Authorize(Roles =nameof(AppRoles.Admin))] // Good practice to also restrict controller action access
        public async Task<IActionResult> Index()
        {
            // Get a list of all users assigned to the "Doctor" role
            var doctors = await _userManager.GetUsersInRoleAsync(AppRoles.Doctor.ToString());

            // Pass the list of doctor users to the view
            return View(doctors);
        }
        // GET: Doctor/CreateDoctor
        public IActionResult CreateDoctor()
        {
            return View();
        }

        // POST: Doctor/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Ensure "Doctor" role exists
                    if (!await _roleManager.RoleExistsAsync("Doctor"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                    }
                    _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user)).Wait();

                    // Assign "Doctor" role to the user
                    await _userManager.AddToRoleAsync(user, "Doctor");

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        // Details endpoint for a patient
        // GET: UserManagement/Details/id
        // GET: UserManagement/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var patients = await db.Patients
                .Where(p => p.DocID == id)
                .ToListAsync();

            var viewModel = new DoctorDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roles.Contains("Doctor") ? "Doctor" : "None",
                Patients = patients
            };

            return View(viewModel);
        }

        // Edit endpoint for a patient
        [Authorize(Roles = "Doctor")]
        public ActionResult Edit(int id)
        {
            var patient = db.Patients.Find(id);
           
            return View(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Patient patient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(patient).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Patients");
            }
            return View(patient);
        }

        // Delete endpoint for a patient
        [Authorize(Roles = "Doctor")]
        public ActionResult Delete(int id)
        {
            var patient = db.Patients.Find(id);
           
            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var patient = db.Patients.Find(id);
            if (patient != null)
            {
                db.Patients.Remove(patient);
                db.SaveChanges();
            }
            return RedirectToAction("Patients");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}