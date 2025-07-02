using DoctorPatientDashboard.Models;
using DoctorPatientDashboard.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// Note: Ensure your ApplicationUser class exists and is correctly configured in Program.cs
// public class ApplicationUser : IdentityUser { }

namespace DoctorPatientDashboard.Controllers
{
    /// <summary>
    /// Manages user registration, login, and logout for a traditional MVC application.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Register
        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            var roles = Enum.GetValues(typeof(AppRoles))
        .Cast<AppRoles>()
        .Select(r => new SelectListItem
        {
            Value = r.ToString(),
            Text = r.ToString()
        }).ToList();

            var viewModel = new RegisterViewModel
            {
                Roles = roles
            };

            return View(viewModel);
        }

        // POST: /Account/Register
        /// <summary>
        /// Handles the submission of the registration form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email ,FullName = model.Email};
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //confirm the user email
                    _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user)).Wait();

                    // Optionally, you can assign roles to the user here if needed
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    // Sign in the user after they have successfully registered
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // If registration fails, add errors to the model state and return the view
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If model state is not valid, return the view with the model to display validation errors
            return View(model);
        }

        // GET: /Account/Login
        /// <summary>
        /// Displays the user login form.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        /// <summary>
        /// Handles the submission of the login form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Dashboard", "Home");
                }

                // If login fails, add a generic error message and return the view
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            // If model state is not valid, return the view with the model to display validation errors
            return View(model);
        }

        // POST: /Account/Logout
        /// <summary>
        /// Logs the user out.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }

    #region ViewModels

    /// <summary>
    /// Data model for the user registration view.
    /// </summary>
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Select Role")]
        public string SelectedRole { get; set; }

        public List<SelectListItem>? Roles { get; set; }
    }

    /// <summary>
    /// Data model for the user login view.
    /// </summary>
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    #endregion
}
