namespace ClinicManagementSystem.API.Interfaces;

public interface INotificationEmailDispatcher
{
    /// Sends pending notification emails. Returns how many were sent successfully.
    Task<int> DispatchPendingAsync(CancellationToken ct);
}