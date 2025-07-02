using DoctorPatientDashboard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace DoctorPatientDashboard.Controllers
{
    public class TestController : Controller
    {
        private readonly EntitiesContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TestController(EntitiesContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> Tests(int patientId)
        {
            
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            
            var tests = await _context.Tests
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.Date) 
                .ToListAsync();

           
            ViewData["PatientName"] = patient.Name;
            ViewData["PatientId"] = patient.PatientID;

            return View(tests);
        }

        public async Task<IActionResult> Create(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            
            ViewData["PatientName"] = patient.Name;
            ViewData["PatientId"] = patient.PatientID;

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId, IFormFile testImageFile)
        {
            
            if (testImageFile != null && testImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Image");

                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(testImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await testImageFile.CopyToAsync(fileStream);
                }

                
                var newTest = new Test
                {
                    PatientId = patientId,
                    TestImage = "/Image/" + uniqueFileName, 
                    Date = DateTime.UtcNow
                };

                
                _context.Tests.Add(newTest);
                await _context.SaveChangesAsync();

                await UpdatePatientDiagnosis(patientId);
                return RedirectToAction("Tests", new { patientId = patientId });
            }

            
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction("Create", new { patientId = patientId });
        }

        public async Task<IActionResult> Analyze(int testId) 
        {
            
            var test = await _context.Tests.Include(t => t.Patient).FirstOrDefaultAsync(t => t.TestID == testId);
            if (test == null)
            {
                return NotFound();
            }

            
            byte[] imageBytes = System.IO.File.ReadAllBytes(Path.Combine(_hostEnvironment.WebRootPath, test.TestImage.TrimStart('/')));

            using (var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageBytes), "file", "test_image.png");

                try
                {
                    var response = await client.PostAsync("http://localhost:5000/predict", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        
                        TempData["ErrorMessage"] = "Error connecting to the AI analysis service.";
                        return RedirectToAction("Index", new { patientId = test.PatientId });
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    
                    var diagnosis = JsonSerializer.Deserialize<Dictionary<string, string>>(result)["diagnosis"];

                    
                    test.Result = diagnosis;

                    
                    test.Patient.Diagnosis = diagnosis;

                    
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    
                    TempData["ErrorMessage"] = "An error occurred during analysis: " + ex.Message;
                }
            }

            
            return RedirectToAction("Tests", new { patientId = test.PatientId });
        }

        public async Task<IActionResult> Edit(int id) 
        {
            var test = await _context.Tests
                                     .Include(t => t.Patient)
                                     .FirstOrDefaultAsync(t => t.TestID == id); 

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Test testModel, IFormFile newTestImage)
        {
            if (id != testModel.TestID)
            {
                return NotFound();
            }

            
            if (newTestImage == null || newTestImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a new image to upload.";
                
                var patient = await _context.Patients.FindAsync(testModel.PatientId);
                testModel.Patient = patient;
                return View(testModel);
            }

            var originalTest = await _context.Tests.AsNoTracking().FirstOrDefaultAsync(t => t.TestID == id);
            if (originalTest == null)
            {
                return NotFound();
            }

            
            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "Image");
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newTestImage.FileName);
            var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(newFilePath, FileMode.Create))
            {
                await newTestImage.CopyToAsync(fileStream);
            }

            
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, originalTest.TestImage.TrimStart('/'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            
            originalTest.TestImage = "/Image/" + uniqueFileName; 
            originalTest.Result = null; 

            try
            {
                _context.Update(originalTest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
            }

            await UpdatePatientDiagnosis(originalTest.PatientId);
            return RedirectToAction(nameof(Tests), new { patientId = originalTest.PatientId });
        }

        [HttpPost] 
        [ValidateAntiForgeryToken]
        [HttpPost]
        
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

      
            var patientId = test.PatientId;


            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, test.TestImage.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();

            
            await UpdatePatientDiagnosis(patientId);

            
            return RedirectToAction(nameof(Tests), new { patientId = patientId });
        }

        private async Task UpdatePatientDiagnosis(int patientId)
        {
            
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return;

            
            var tests = await _context.Tests
                                      .Where(t => t.PatientId == patientId)
                                      .OrderByDescending(t => t.Date)
                                      .ToListAsync();

            string newDiagnosis = "Pending Analysis..."; 

            if (tests.Any()) 
            {
                
                var latestTestWithResult = tests.FirstOrDefault(t => !string.IsNullOrEmpty(t.Result));

                if (latestTestWithResult != null)
                {
                    
                    newDiagnosis = latestTestWithResult.Result;
                }
  
            }

            
            patient.Diagnosis = newDiagnosis;
            await _context.SaveChangesAsync();
        }

    }
}
