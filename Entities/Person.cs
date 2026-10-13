using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.API.Entities;

public partial class Person
{
    public int PersonId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}
