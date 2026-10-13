using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class MedicalRecord
{
    public int MedicalRecordId { get; set; }

    public string? VisitDescription { get; set; }

    public string? Diagnosis { get; set; }

    public string? AdditionalNotes { get; set; }

    public Appointment? Appointment { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
