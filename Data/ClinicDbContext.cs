using System;
using System.Collections.Generic;
using ClinicManagementSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API;

public partial class ClinicDbContext : DbContext
{
  
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<DoctorWorkingHour> DoctorWorkingHours => Set<DoctorWorkingHour>();
    public DbSet<DoctorBreak> DoctorBreaks => Set<DoctorBreak>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
    public DbSet<DoctorQualification> DoctorQualifications => Set<DoctorQualification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);
    }

  
}
