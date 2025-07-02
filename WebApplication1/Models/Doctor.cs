using System.ComponentModel.DataAnnotations;


namespace DoctorPatientDashboard.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Account { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsManager { get; set; }

        public int Age { get; set; }
    }
}
