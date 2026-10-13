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
        public async Task<ActionResult<IReadOnlyList<DoctorListItemDto>>> GetDoctors([FromQuery]string? search, [FromQuery] string? specialization, CancellationToken ct)
        {


            if (search?.Length > 100 || specialization?.Length > 100)
                return BadRequest("Search values must be 100 characters or fewer");


                return Ok(await doctorService.GetDoctorsAsync(search, specialization, ct));

        }

        [HttpGet("specializations")]
        public async Task<ActionResult<IReadOnlyList<string>>>GetSpecialization(CancellationToken ct)=>
            Ok(await doctorService.GetSpecializationAsync(ct)); 





    }
}
