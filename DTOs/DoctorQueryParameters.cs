using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.API.DTOs;

public class DoctorQueryParameters
{
    public const int MaxPageSize = 50;

    [MaxLength(100)]
    public string? Search { get; init; }

    [MaxLength(100)]
    public string? Specialization { get; init; }

    [Range(1, 100_000)]
    public int PageNumber { get; init; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = 10;

    public DoctorSortBy SortBy { get; init; } = DoctorSortBy.Name;

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}