using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Patient
{
    public int PatientId { get; set; }

    public int PersonId { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Person Person { get; set; } = null!;
}
