using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoctorPatientDashboard.Controllers
{
    public class DoctorController : Controller
    {
        private readonly EntitiesContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DoctorController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EntitiesContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = context;
        }

        [Authorize(Roles = nameof(AppRoles.Admin))]
        public async Task<IActionResult> Index()
        {
            var doctors = await _userManager.GetUsersInRoleAsync(AppRoles.Doctor.ToString());
            return View(doctors);
        }

        public IActionResult CreateDoctor()
        {
            return View();
        }

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
                    if (!await _roleManager.RoleExistsAsync("Doctor"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                    }
                    _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user)).Wait();
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
            var patients = await db.Patients.Where(p => p.DocID == id).ToListAsync();

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

        // This is the first Edit action you wanted to keep
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDoctor(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allDoctors = await _userManager.GetUsersInRoleAsync("Doctor");

            var viewModel = new DoctorEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SelectedRole = userRoles.FirstOrDefault(),
                AllRoles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync(),
                AssignedPatients = await db.Patients.Where(p => p.DocID == id).ToListAsync(),
                OtherDoctors = allDoctors
                    .Where(d => d.Id != id)
                    .Select(d => new SelectListItem { Value = d.Id, Text = d.FullName })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDoctor(DoctorEditViewModel model)
        {
           
            if (!ModelState.IsValid)
            {
               
                var allDoctors = await _userManager.GetUsersInRoleAsync("Doctor");
                model.AllRoles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();
                model.AssignedPatients = await db.Patients.Where(p => p.DocID == model.Id).ToListAsync();
                model.OtherDoctors = allDoctors
                    .Where(d => d.Id != model.Id)
                    .Select(d => new SelectListItem { Value = d.Id, Text = d.FullName })
                    .ToList();

                return View("EditDoctor", model); 
            }

           
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
           

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

                return RedirectToAction("Index");
            }

            
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            
            var finalAllDoctors = await _userManager.GetUsersInRoleAsync("Doctor");
            model.AllRoles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();
            model.AssignedPatients = await db.Patients.Where(p => p.DocID == model.Id).ToListAsync();
            model.OtherDoctors = finalAllDoctors
                .Where(d => d.Id != model.Id)
                .Select(d => new SelectListItem { Value = d.Id, Text = d.FullName })
                .ToList();

            return View("EditDoctor", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReassignPatient(int patientId, string newDoctorId)
        {
            var patient = await db.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return Json(new { success = false, message = "Patient not found." });
            }

            patient.DocID = newDoctorId;
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Patient reassigned successfully." });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var doctor = await _userManager.FindByIdAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["PatientCount"] = await db.Patients.CountAsync(p => p.DocID == id);
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var doctor = await _userManager.FindByIdAsync(id);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }
            var assignedPatients = await db.Patients.Where(p => p.DocID == id).ToListAsync();
            foreach (var patient in assignedPatients)
            {
                patient.DocID = null;
            }
            var result = await _userManager.DeleteAsync(doctor);
            if (result.Succeeded)
            {
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewData["PatientCount"] = assignedPatients.Count;
            return View(doctor);
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