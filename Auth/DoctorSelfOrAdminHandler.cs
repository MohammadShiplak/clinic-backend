// Auth/DoctorSelfOrAdminHandler.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManagementSystem.API.Auth;

public sealed class DoctorSelfOrAdminRequirement : IAuthorizationRequirement { }

public class DoctorSelfOrAdminHandler : AuthorizationHandler<DoctorSelfOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DoctorSelfOrAdminRequirement requirement)
    {
        // With endpoint routing, the resource is the HttpContext, which gives access to route values.
        if (context.Resource is not HttpContext http) return Task.CompletedTask;

        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(Roles.Doctor)
            && int.TryParse(http.Request.RouteValues["doctorId"]?.ToString(), out var routeDoctorId)
            && int.TryParse(context.User.FindFirstValue(AppClaimTypes.DoctorId), out var myDoctorId)
            && routeDoctorId == myDoctorId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;   // not succeeding = denied
    }
}