using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.API.DTOs
{
    public class AppointmentDtos
    {

public record SlotDto(DateTime Start,DateTime End);

        public record AppointmentDto(int AppointmentId, int DoctorId,string DoctorName,
            DateTime AppointmentDateTime,string Status);


        public class CreateAppointmentRequest
        {

            [Range(1, int.MaxValue)] public int DoctorId { get; init; }
            [Range(1, int.MaxValue)] public int PatientId { get; init; }   // TEMPORARY: will come from the JWT
            public DateTime AppointmentDateTime { get; init; }




        }






    }
}
