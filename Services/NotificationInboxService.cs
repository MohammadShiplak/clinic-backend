using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services;

public class NotificationInboxService(ClinicDbContext context, TimeProvider clock) : INotificationInboxService
{
    public async Task<PagedResult<NotificationDto>> GetAsync(
        int personId, bool unreadOnly, int pageNumber, int pageSize, CancellationToken ct)
    {
        // Security: every query starts from the owner.
        var query = context.Notifications.AsNoTracking().Where(n => n.PersonId == personId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc).ThenByDescending(n => n.NotificationId) // stable paging
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.NotificationId, n.AppointmentId, n.Type,
                n.Title, n.Message, n.IsRead, n.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<NotificationDto>(items, pageNumber, pageSize, totalCount);
    }

    public Task<int> GetUnreadCountAsync(int personId, CancellationToken ct) =>
        context.Notifications.AsNoTracking()
            .CountAsync(n => n.PersonId == personId && !n.IsRead, ct);

    public async Task<bool> MarkAsReadAsync(int personId, int notificationId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var updated = await context.Notifications
            .Where(n => n.NotificationId == notificationId && n.PersonId == personId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAtUtc, now), ct);

        if (updated > 0) return true;

        // 0 rows changed: it either isn't yours / doesn't exist, or it was already read.
        return await context.Notifications
            .AnyAsync(n => n.NotificationId == notificationId && n.PersonId == personId, ct);
    }

    public async Task MarkAllAsReadAsync(int personId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        await context.Notifications
            .Where(n => n.PersonId == personId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAtUtc, now), ct);
    }
}