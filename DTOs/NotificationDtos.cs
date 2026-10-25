using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.DTOs;

public record NotificationDto(
    int NotificationId,
    int? AppointmentId,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc);

public record UnreadCountDto(int Count);