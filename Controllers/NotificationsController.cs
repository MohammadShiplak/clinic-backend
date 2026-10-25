using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationInboxService inbox) : ControllerBase
{
    private const int MaxPageSize = 50;

    // TEMPORARY: personId will come from the JWT. Only this controller needs to change.

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] int personId,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (personId < 1)
            return BadRequest("personId is required.");

        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"pageNumber must be at least 1 and pageSize between 1 and {MaxPageSize}.");

        return Ok(await inbox.GetAsync(personId, unreadOnly, pageNumber, pageSize, ct));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(
        [FromQuery] int personId, CancellationToken ct)
    {
        if (personId < 1)
            return BadRequest("personId is required.");

        return Ok(new UnreadCountDto(await inbox.GetUnreadCountAsync(personId, ct)));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, [FromQuery] int personId, CancellationToken ct)
    {
        if (personId < 1)
            return BadRequest("personId is required.");

        return await inbox.MarkAsReadAsync(personId, id, ct) ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] int personId, CancellationToken ct)
    {
        if (personId < 1)
            return BadRequest("personId is required.");

        await inbox.MarkAllAsReadAsync(personId, ct);
        return NoContent();
    }
}