using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Buffers.Text;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class PatientsController : Controller
{
    private readonly EntitiesContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientsController(EntitiesContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Patients (List all patients)
    public async Task<IActionResult> Index()
    {
        List<Patient> patients;

        if (User.IsInRole(AppRoles.Doctor.ToString()))
        {
            var id = _userManager.GetUserId(User);
            patients = _context.Patients.Include(p=>p.Doctor).Where(p => p.DocID == id).ToList();
        }
        else if (User.IsInRole(AppRoles.Admin.ToString()))
        {
            patients = _context.Patients.Include(p => p.Doctor).ToList();
        }
        else
        {
            // Unauthorized access
            return Unauthorized();
        }

        return View(patients);
    }

    // GET: Patients/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var patient = await _context.Patients
            .Include(p => p.Doctor) // Include doctor details
            .FirstOrDefaultAsync(m => m.PatientID == id);

        if (patient == null) return NotFound();

        return View(patient);
    }

    // GET: Patients/Create
    public async Task<IActionResult> Create()
    {
        // Pass the list of doctors to the view for a dropdown
        ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName");
        return View();
    }

    // POST: Patients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    // Only bind the fields present in the form to prevent over-posting
    [Bind("Name,Age,Address,Phone,Gender,DocID")] Patient patient)
    {
        // Check if the current user is a Doctor
        if (User.IsInRole("Doctor"))
        {
            // **Security Step:**
            // Automatically assign the logged-in doctor's ID to the patient.
            // This overrides any value that might be sent in the form.
            patient.DocID = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Set the fields that are not in the form
        patient.Date = DateTime.UtcNow; // Set the creation date to the current time
        patient.Diagnosis = null;       // Ensure Diagnosis starts as null

        if (ModelState.IsValid)
        {
            _context.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // If the model is not valid (e.g., Name is missing), and the user is an Admin, 
        // we must reload the doctor list before showing the form again.
        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patient.DocID);
        }

        return View(patient);
    }

  
// GET: Patients/Edit/5
public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // **Security Check:** An Admin can edit any patient.
        // A Doctor can only edit patients currently assigned to them.
        if (!User.IsInRole("Admin") && patient.DocID != currentUserId)
        {
            return Forbid(); // Or return a custom "Access Denied" view
        }

        // If the user is an Admin, pass the doctor list to the view,
        // pre-selecting the patient's current doctor.
        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patient.DocID);
        }

        return View(patient);
    }

    // POST: Patients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PatientID,Name,Age,Address,Phone,Gender,DocID")] Patient patient)
    {
        if (id != patient.PatientID) return NotFound();

        // **Security Check:** Before saving, verify the user is still authorized.
        // This prevents a user from submitting an edit for a patient they lost access to.
        var originalPatient = await _context.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientID == id);
        if (originalPatient == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && originalPatient.DocID != currentUserId)
        {
            return Forbid();
        }

        // If the user is a doctor, ensure they cannot re-assign the patient.
        // We restore the original DocID to prevent tampering.
        if (User.IsInRole("Doctor"))
        {
            patient.DocID = originalPatient.DocID;
        }

        // Preserve the original creation date
        patient.Date = originalPatient.Date;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Patients.Any(e => e.PatientID == patient.PatientID))
                {
                    return NotFound();
                }
                else { throw; }
            }
            return RedirectToAction(nameof(Index));
        }

        // If model state is invalid and user is an Admin, reload the dropdown
        if (User.IsInRole("Admin"))
        {
            ViewData["DocID"] = new SelectList(await _userManager.GetUsersInRoleAsync("Doctor"), "Id", "UserName", patient.DocID);
        }

        return View(patient);
    }

    // GET: Patients/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .FirstOrDefaultAsync(m => m.PatientID == id);

        if (patient == null) return NotFound();

        return View(patient);
    }

    // POST: Patients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}