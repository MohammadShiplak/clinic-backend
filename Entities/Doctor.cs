using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int PersonId { get; set; }

    public string? Specialization { get; set; }

    public DateOnly? CareerStartDate { get; set; }

    public string? Biography { get; set; }  

    public string? ClinicRoom { get; set; } 


    public string? ProfileImageUrl { get; set; }    


    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<DoctorQualification>Qualifications { get; set; } 

    public virtual ICollection<DoctorWorkingHour>WorkingHours { get; set; }=new List<DoctorWorkingHour>();  

    public virtual ICollection<DoctorBreak> Breaks { get; set; }=new List<DoctorBreak>();   

    public virtual ICollection<DoctorTimeOff> TimeOff { get; set; } = new List<DoctorTimeOff>();    




    public virtual Person Person { get; set; } = null!;
}
