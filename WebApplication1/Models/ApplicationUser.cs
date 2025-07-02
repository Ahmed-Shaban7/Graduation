
using Microsoft.AspNetCore.Identity;

namespace DoctorPatientDashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties if needed
        public string FullName { get; set; }

        // relationships can be added here if needed for example: patients

    }
}
