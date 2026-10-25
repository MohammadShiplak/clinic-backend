using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces;

public interface INotificationInboxService
{
    Task<PagedResult<NotificationDto>> GetAsync(
        int personId, bool unreadOnly, int pageNumber, int pageSize, CancellationToken ct);

    Task<int> GetUnreadCountAsync(int personId, CancellationToken ct);

    /// false = not found (or not yours). Marking an already-read item is a success.
    Task<bool> MarkAsReadAsync(int personId, int notificationId, CancellationToken ct);

    Task MarkAllAsReadAsync(int personId, CancellationToken ct);
}