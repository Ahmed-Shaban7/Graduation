using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

public class PatientsController : Controller
{
    private readonly EntitiesContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientsController(EntitiesContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        List<Patient> patients;

        if (User.IsInRole(AppRoles.Doctor.ToString()))
        {
            var id = _userManager.GetUserId(User);
            patients = await _context.Patients.Include(p => p.Doctor).Where(p => p.DocID == id).ToListAsync();
        }
        else if (User.IsInRole(AppRoles.Admin.ToString()))
        {
            patients = await _context.Patients.Include(p => p.Doctor).ToListAsync();
        }
        else
        {
            return Unauthorized();
        }

        return View(patients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .FirstOrDefaultAsync(m => m.PatientID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Age,Address,Phone,Gender,DocID")] Patient patient)
    {
        if (User.IsInRole("Doctor"))
        {
            patient.DocID = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        patient.Date = DateTime.UtcNow;
        patient.Diagnosis = null;

        if (ModelState.IsValid)
        {
            _context.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patient.DocID);
        }

        return View(patient);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && patient.DocID != currentUserId)
        {
            return Forbid();
        }

        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patient.DocID);
        }

        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var patientToUpdate = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == id);

        if (patientToUpdate == null)
        {
            return NotFound();
        }

        bool isUpdated = await TryUpdateModelAsync<Patient>(
            patientToUpdate,
            "",
            p => p.Name, p => p.Age, p => p.Address, p => p.Phone, p => p.Gender, p => p.DocID);

        if (isUpdated)
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save changes. The record was modified by another user.");
            }
        }

        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patientToUpdate.DocID);
        }
        return View(patientToUpdate);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .FirstOrDefaultAsync(m => m.PatientID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}