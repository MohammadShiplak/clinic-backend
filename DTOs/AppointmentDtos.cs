using ClinicManagementSystem.API.Entities;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.API.DTOs
{
    public class AppointmentDtos
    {

        public enum AppointmentScope { Upcoming=1,Past=2}

        public enum CancelOutcome { Cancelled,NotFound,NotEligible}


        public record PagedResult<T>(IReadOnlyList<T>Items,int PageNumber,int PageSize,int TotalCount)
        {
            public int TotalPages =>(int)Math.Ceiling(TotalCount/(double)PageSize);
        }



public record SlotDto(DateTime Start,DateTime End);

        public record AppointmentDto(int AppointmentId, int DoctorId,string DoctorName,
            DateTime AppointmentDateTime,string Status);


        // What a LIST card needs: small and cheap.
        public record AppointmentListItemDto(
            int AppointmentId,
            int DoctorId,
            string DoctorName,
            string? Specialization,
            string? DoctorImageUrl,
            DateTime AppointmentDateTime,
            AppointmentStatus Status,
            bool CanCancel);

        public record PrescriptionDto(
            string MedicationName,
            string Dosage,
            string Frequency,
            DateOnly StartDate,
            DateOnly EndDate,
            string? SpecialInstructions);

        public record PaymentDto(
            DateOnly PaymentDate,
            string? PaymentMethod,
            decimal AmountPaid);

        // What the DETAILS page needs: the list fields plus medical record and payment.
        public record AppointmentDetailsDto(
            int AppointmentId,
            int DoctorId,
            string DoctorName,
            string? Specialization,
            string? DoctorImageUrl,
            DateTime AppointmentDateTime,
            AppointmentStatus Status,
            bool CanCancel,
            string? VisitDescription,
            string? Diagnosis,
            string? AdditionalNotes,
            IReadOnlyList<PrescriptionDto> Prescriptions,
            PaymentDto? Payment);



        public class CreateAppointmentRequest
        {

            [Range(1, int.MaxValue)] public int DoctorId { get; init; }
            [Range(1, int.MaxValue)] public int PatientId { get; init; }   // TEMPORARY: will come from the JWT
            public DateTime AppointmentDateTime { get; init; }




        }






    }
}
