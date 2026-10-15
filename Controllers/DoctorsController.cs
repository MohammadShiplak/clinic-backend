using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController(IDoctorService doctorService) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<PagedResult<DoctorListItemDto>>> GetDoctors(
       [FromQuery] DoctorQueryParameters query, CancellationToken ct)
       => Ok(await doctorService.GetDoctorsAsync(query, ct));

        [HttpGet("specializations")]
        public async Task<ActionResult<IReadOnlyList<string>>>GetSpecialization(CancellationToken ct)=>
            Ok(await doctorService.GetSpecializationAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DoctorDetailsDto>>GetDoctorById(int id, CancellationToken ct)
        {
            var doctor =await doctorService.GetDoctorByIdAsync(id, ct);

            return doctor is null ? NotFound(): Ok(doctor); 
        }




    }
}
