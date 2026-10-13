using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int MedicalRecordId { get; set; }

    public string MedicationName { get; set; } = null!;

    public string Dosage { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? SpecialInstructions { get; set; }

    public virtual MedicalRecord MedicalRecord { get; set; } = null!;
}
