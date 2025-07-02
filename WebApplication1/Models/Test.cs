using System.ComponentModel.DataAnnotations;


namespace DoctorPatientDashboard.Models
{
    public class Test
    {
        [Key]
        public int TestID { get; set; }

        public string? Result { get; set; }

        public DateTime Date { get; set; }

        public int PatientId { get; set; }

        public string TestImage { get; set; }

        public virtual Patient Patient { get; set; }
    }
}
