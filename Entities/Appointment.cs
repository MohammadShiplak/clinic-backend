using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime AppointmentDateTime { get; set; }

  public AppointmentStatus Status { get; set; } 

    public int? MedicalRecordId { get; set; }

    public int? PaymentId { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Payment? Payment { get; set; }
}

public enum AppointmentStatus:byte
{

    Scheduled =1,
    Completed =2,
    Cancelled = 3,
    NoShow=4

}
