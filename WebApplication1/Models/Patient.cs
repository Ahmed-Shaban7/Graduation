using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DoctorPatientDashboard.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        [Required]
        public string Name { get; set; }

        public int? Age { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public string? Diagnosis { get; set; }

        public DateTime Date { get; set; }

        public string? DocID { get; set; } // Foreign key to the User table
        [ForeignKey("DocID")]
        public ApplicationUser? Doctor { get; set; } // Navigation property to the User table
    }
}
