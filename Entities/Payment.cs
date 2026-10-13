using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public DateOnly PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal AmountPaid { get; set; }

    public string? AdditionalNotes { get; set; }

    public Appointment? Appointment { get; set; }
}
