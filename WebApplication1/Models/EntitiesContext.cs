using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorPatientDashboard.Models
{
    public class EntitiesContext : IdentityDbContext<ApplicationUser>
    {
        public EntitiesContext(DbContextOptions<EntitiesContext> options)
               : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Test> Tests { get; set; }
    }
}
