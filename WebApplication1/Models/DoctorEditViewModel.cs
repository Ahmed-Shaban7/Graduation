using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DoctorPatientDashboard.Models
{
    public class DoctorEditViewModel
    {
        // Doctor's Information
        public string Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }

        // Role Management
        [Required, Display(Name = "Role")]
        public string SelectedRole { get; set; }
        public List<SelectListItem> AllRoles { get; set; }

        // Patient Management
        public List<Patient> AssignedPatients { get; set; }
        public List<SelectListItem> OtherDoctors { get; set; }
    }
}
