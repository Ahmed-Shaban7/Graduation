namespace DoctorPatientDashboard.Models
{
    public class DoctorDetailsViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public List<Patient> Patients { get; set; }
    }
}
