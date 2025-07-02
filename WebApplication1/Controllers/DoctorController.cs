using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace DoctorPatientDashboard.Controllers
{
    public class DoctorController : Controller
    {
        private readonly EntitiesContext db ;
        private readonly UserManager<ApplicationUser> _userManager;
        public DoctorController(UserManager<ApplicationUser> userManager,EntitiesContext context)
        {
            _userManager = userManager;
            db = context;
        }

        [Authorize(Roles =nameof(AppRoles.Admin))] // Good practice to also restrict controller action access
        public async Task<IActionResult> Doctors()
        {
            // Get a list of all users assigned to the "Doctor" role
            var doctors = await _userManager.GetUsersInRoleAsync(AppRoles.Doctor.ToString());

            // Pass the list of doctor users to the view
            return View(doctors);
        }
     
        // Details endpoint for a patient
        [Authorize(Roles = "Doctor")]
        public ActionResult Details(int id)
        {
            var patient = db.Patients.Find(id);
           
            return View(patient);
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

        // Tests endpoint for a patient
        [Authorize(Roles = "Doctor")]
        public ActionResult Tests(int patientId)
        {
            var patient = db.Patients.Find(patientId);
            
            var tests = db.Tests.Where(t => t.PatientId == patientId).ToList();
            return View(tests);
        }

        // Edit endpoint for a test
        [Authorize(Roles = "Doctor")]
        public ActionResult EditTest(int id)
        {
            var test = db.Tests.Find(id);
            
            return View(test);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult EditTest(Test test)
        {
            if (ModelState.IsValid)
            {
                db.Entry(test).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Tests", new { patientId = test.PatientId });
            }
            return View(test);
        }

        // Delete endpoint for a test
        [Authorize(Roles = "Doctor")]
        public ActionResult DeleteTest(int id)
        {
            var test = db.Tests.Find(id);
           
            return View(test);
        }

        [HttpPost, ActionName("DeleteTest")]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTestConfirmed(int id)
        {
            var test = db.Tests.Find(id);
            if (test != null)
            {
                int patientId = test.PatientId;
                db.Tests.Remove(test);
                db.SaveChanges();
                return RedirectToAction("Tests", new { patientId = patientId });
            }
            return RedirectToAction("Tests");
        }

        // Analyze endpoint to link with Flask
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> Analyze(int testId)
        {
            var test = db.Tests.Find(testId);
           

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("image", test.TestImage)
                });
                var response = await client.PostAsync("http://localhost:5000/analyze", content); // Replace with your Flask URL
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    test.Result = result; // Update test result with Flask analysis
                    db.SaveChanges();
                }
            }
            ViewBag.Status = "Analysis completed";
            return RedirectToAction("Tests", new { patientId = test.PatientId });
        }

        

        // Add a patient (Doctor only)
        [Authorize(Roles = "Doctor")]
        public ActionResult AddPatient()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPatient(Patient patient)
        {
            if (ModelState.IsValid)
            {
               
             
                    patient.Date = DateTime.Now;
                    db.Patients.Add(patient);
                    db.SaveChanges();
                    return RedirectToAction("Patients");
             
            }
            return View(patient);
        }

        // Add a test for a patient (Doctor only)
        [Authorize(Roles = "Doctor")]
        public ActionResult AddTest(int patientId)
        {
            ViewBag.PatientId = patientId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public ActionResult AddTest(Test test)
        {
            if (ModelState.IsValid)
            {
                test.Date = DateTime.Now;
                db.Tests.Add(test);
                db.SaveChanges();
                return RedirectToAction("Tests", new { patientId = test.PatientId });
            }
            return View(test);
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