namespace ClinicManagementSystem.API.DTOs;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}