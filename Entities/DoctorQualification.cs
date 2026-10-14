namespace ClinicManagementSystem.API.Entities
{
    public class DoctorQualification
    {

        public int DoctorQualificationId { get; set; }  

        public int DoctorId { get; set; }   

        public string Title { get; set; }   

        public string? Institution { get; set; }    

        public int? YearAwarded { get; set; }

        public virtual Doctor Doctor { get; set; } = null;





    }
}
