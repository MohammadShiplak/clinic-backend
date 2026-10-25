namespace ClinicManagementSystem.API.Entities
{
    public class DoctorTimeOff
    {
        public int DoctorTimeOffId { get; set; }    
        public int DoctorId { get; set; }

       

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string? Reason { get; set; }     

        public virtual Doctor Doctor { get; set; } = null;
    }
}
