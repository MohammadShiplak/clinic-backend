using System.Security.Claims;

namespace ClinicManagementSystem.API.Auth
{
    public interface ICurrentUser
    {

        int? PatientId { get; }
        int? DoctorId { get; }

       bool IsAdmin { get; }    
    }

    public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {

        private ClaimsPrincipal principal =>
            accessor.HttpContext?.User ?? throw new InvalidOperationException("No active http context.");


        public int? PatientId => ReadId(AppClaimTypes.PatientId);

        public int? DoctorId => ReadId(AppClaimTypes.DoctorId);

        public bool IsAdmin => principal.IsInRole(Roles.Admin);



        private int? ReadId(string claim) =>
            int.TryParse(principal.FindFirstValue(claim), out var id) ? id : null;


    }



}
