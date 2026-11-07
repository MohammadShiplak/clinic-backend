using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]   // any logged-in user: patients and doctors both have notifications
public class NotificationsController(INotificationInboxService inbox) : ControllerBase
{
    private const int MaxPageSize = 50;

    private int PersonId => User.GetRequiredPersonId();

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"pageNumber must be at least 1 and pageSize between 1 and {MaxPageSize}.");

        return Ok(await inbox.GetAsync(PersonId, unreadOnly, pageNumber, pageSize, ct));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(CancellationToken ct) =>
        Ok(new UnreadCountDto(await inbox.GetUnreadCountAsync(PersonId, ct)));

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct) =>
        await inbox.MarkAsReadAsync(PersonId, id, ct) ? NoContent() : NotFound();

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await inbox.MarkAllAsReadAsync(PersonId, ct);
        return NoContent();
    }
}