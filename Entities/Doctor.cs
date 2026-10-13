using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int PersonId { get; set; }

    public string? Specialization { get; set; }

    public DateOnly? CareerStarDate { get; set; }

    public string? ProfileImageUrl { get; set; }    


    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Person Person { get; set; } = null!;
}
